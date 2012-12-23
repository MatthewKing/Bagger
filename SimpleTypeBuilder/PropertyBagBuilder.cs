namespace SimpleTypeBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Provides functionality to define and build a property bag type.
    /// </summary>
    public sealed class PropertyBagBuilder
    {
        /// <summary>
        /// Name of the type that will be built.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// List of properties that will be included in the type that will be built.
        /// </summary>
        private readonly IList<PropertyDefinition> properties;

        /// <summary>
        /// Initializes a new instance of the PropertyBagBuilder class.
        /// </summary>
        public PropertyBagBuilder()
            : this(GenerateUniqueName("DynamicType")) { }

        /// <summary>
        /// Initializes a new instance of the PropertyBagBuilder class.
        /// </summary>
        /// <param name="name">The name of the type that will be built.</param>
        public PropertyBagBuilder(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name", "name should not be null.");
            if (name.Length == 0)
                throw new ArgumentException("name should not be an empty string.", "name");

            this.name = name;
            this.properties = new List<PropertyDefinition>();
        }

        /// <summary>
        /// Adds a property to this PropertyBagBuilder.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <param name="type">Type of the property.</param>
        /// <returns>This PropertyBagBuilder instance.</returns>
        public PropertyBagBuilder AddProperty(string name, Type type)
        {
            if (name == null)
                throw new ArgumentNullException("name", "name should not be null.");
            if (name.Length == 0)
                throw new ArgumentException("name should not be an empty string.", "name");
            if (type == null)
                throw new ArgumentNullException("type", "type should not be null.");

            foreach (PropertyDefinition property in this.properties)
            {
                if (String.Equals(name, property.Name))
                {
                    string message = String.Format("Property '{0}' is already defined.", name);
                    string paramName = "name";
                    throw new ArgumentException(message, paramName);
                }
            }

            this.properties.Add(new PropertyDefinition(name, type));

            return this;
        }

        /// <summary>
        /// Builds a Type representing the property bag defined by this PropertyBagBuilder.
        /// </summary>
        /// <returns>
        /// A Type representing the property bag defined by this PropertyBagBuilder.
        /// </returns>
        public Type Build()
        {
            MethodInfo stringEquals = typeof(String).GetMethod(
                "op_Equality",
                new Type[] { typeof(string), typeof(string) });

            AppDomain domain = AppDomain.CurrentDomain;
            AssemblyName assemblyName = new AssemblyName(GenerateUniqueName("DynamicAssembly"));
            AssemblyBuilderAccess access = AssemblyBuilderAccess.Run;
            AssemblyBuilder assemblyBuilder = domain.DefineDynamicAssembly(assemblyName, access);
            string moduleName = GenerateUniqueName("DynamicModule");
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                this.name,
                TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class);

            // Implement IPropertyBag (GetValue / SetValue)
            typeBuilder.AddInterfaceImplementation(typeof(IPropertyBag));

            MethodBuilder getValueMethodBuilder = typeBuilder.DefineMethod(
                "GetValue",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                typeof(object),
                new Type[] { typeof(string) });
            ILGenerator getValueILGenerator = getValueMethodBuilder.GetILGenerator();
            MethodInfo getValueInterfaceMethod = typeof(IPropertyBag).GetMethod("GetValue");
            typeBuilder.DefineMethodOverride(getValueMethodBuilder, getValueInterfaceMethod);

            MethodBuilder setValueMethodBuilder = typeBuilder.DefineMethod(
                "SetValue",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                null,
                new Type[] { typeof(string), typeof(object) });
            ILGenerator setValueILGenerator = setValueMethodBuilder.GetILGenerator();
            MethodInfo setValueInterfaceMethod = typeof(IPropertyBag).GetMethod("SetValue");
            typeBuilder.DefineMethodOverride(setValueMethodBuilder, setValueInterfaceMethod);

            foreach (PropertyDefinition property in this.properties)
            {
                // Define the backing field for the property.
                FieldBuilder field = typeBuilder.DefineField(
                    "_" + property.Name,
                    property.Type,
                    FieldAttributes.Private);

                // Define the getter method for the property.
                MethodBuilder getterMethodBuilder = typeBuilder.DefineMethod(
                    "get_" + property.Name,
                    MethodAttributes.Public | MethodAttributes.HideBySig,
                    property.Type,
                    null);

                ILGenerator getterILGenerator = getterMethodBuilder.GetILGenerator();
                getterILGenerator.Emit(OpCodes.Ldarg_0);
                getterILGenerator.Emit(OpCodes.Ldfld, field);
                getterILGenerator.Emit(OpCodes.Ret);

                // Define the setter method for the property.
                MethodBuilder setterMethodBuilder = typeBuilder.DefineMethod(
                    "set_" + property.Name,
                    MethodAttributes.Public | MethodAttributes.HideBySig,
                    null,
                    new Type[] { property.Type });

                ILGenerator setterILGenerator = setterMethodBuilder.GetILGenerator();
                setterILGenerator.Emit(OpCodes.Ldarg_0);
                setterILGenerator.Emit(OpCodes.Ldarg_1);
                setterILGenerator.Emit(OpCodes.Stfld, field);
                setterILGenerator.Emit(OpCodes.Ret);

                // Create the property, using the generated getter and setter.
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
                    property.Name,
                    PropertyAttributes.HasDefault,
                    property.Type,
                    null);
                propertyBuilder.SetGetMethod(getterMethodBuilder);
                propertyBuilder.SetSetMethod(setterMethodBuilder);

                // Add the resolution of this property to the GetValue method.
                // The rough intent of this code is to generate something similar to:
                //   if (String.Equals(ARGUMENT_PROPERTY_NAME, INLINED_PROPERTY_NAME))
                //   {
                //       return FIELD_VALUE;
                //   }
                Label getValueSkipLabel = getValueILGenerator.DefineLabel();
                getValueILGenerator.Emit(OpCodes.Ldarg_1);
                getValueILGenerator.Emit(OpCodes.Ldstr, property.Name);
                getValueILGenerator.EmitCall(OpCodes.Call, stringEquals, null);
                getValueILGenerator.Emit(OpCodes.Brfalse_S, getValueSkipLabel);
                getValueILGenerator.Emit(OpCodes.Ldarg_0);
                getValueILGenerator.Emit(OpCodes.Ldfld, field);
                if (property.Type.IsValueType)
                    getValueILGenerator.Emit(OpCodes.Box, property.Type);
                getValueILGenerator.Emit(OpCodes.Ret);
                getValueILGenerator.MarkLabel(getValueSkipLabel);

                // Adds this property to the SetValue method.
                // The rough intent of this code is to generate something similar to:
                //   if (String.Equals(ARGUMENT_PROPERTY_NAME, INLINED_PROPERTY_NAME))
                //   {
                //       FIELD_VALUE = ARGUMENT_FIELD_VALUE;
                //       return;
                //   }
                Label setValueSkipLabel = setValueILGenerator.DefineLabel();
                setValueILGenerator.Emit(OpCodes.Ldarg_1);
                setValueILGenerator.Emit(OpCodes.Ldstr, property.Name);
                setValueILGenerator.EmitCall(OpCodes.Call, stringEquals, null);
                setValueILGenerator.Emit(OpCodes.Brfalse_S, setValueSkipLabel);
                setValueILGenerator.Emit(OpCodes.Ldarg_0);
                setValueILGenerator.Emit(OpCodes.Ldarg_2);
                if (property.Type.IsValueType)
                    setValueILGenerator.Emit(OpCodes.Unbox_Any, property.Type);
                else
                    setValueILGenerator.Emit(OpCodes.Castclass, property.Type);
                setValueILGenerator.Emit(OpCodes.Stfld, field);
                setValueILGenerator.Emit(OpCodes.Ret);
                setValueILGenerator.MarkLabel(setValueSkipLabel);
            }

            // If the GetValue and SetValue methods have made it past all of the properties
            // without a match (and thus a return), we'll throw an ArgumentException.
            ConstructorInfo exConstructor = typeof(ArgumentException)
                .GetConstructor(new Type[] { typeof(string), typeof(string) });
            string exMessage = "propertyName must refer to a parameter that exists on this type.";
            string exParameterName = "propertyName";
            getValueILGenerator.Emit(OpCodes.Ldstr, exMessage);
            getValueILGenerator.Emit(OpCodes.Ldstr, exParameterName);
            getValueILGenerator.Emit(OpCodes.Newobj, exConstructor);
            getValueILGenerator.Emit(OpCodes.Throw);
            setValueILGenerator.Emit(OpCodes.Ldstr, exMessage);
            setValueILGenerator.Emit(OpCodes.Ldstr, exParameterName);
            setValueILGenerator.Emit(OpCodes.Newobj, exConstructor);
            setValueILGenerator.Emit(OpCodes.Throw);

            // Build and return the type!
            Type generatedType = typeBuilder.CreateType();
            return generatedType;
        }

        /// <summary>
        /// Generates a unique name, given the specified base name.
        /// </summary>
        /// <param name="baseName">The base name that a unique name will be generated from.</param>
        /// <returns>A unique name.</returns>
        private static string GenerateUniqueName(string baseName)
        {
            if (baseName == null)
                throw new ArgumentNullException("baseName", "baseName should not be null.");
            if (baseName.Length == 0)
                throw new ArgumentException("baseName should not be an empty string.", "baseName");

            return String.Concat(baseName, Guid.NewGuid().ToString("N").ToUpper());
        }
    }
}
