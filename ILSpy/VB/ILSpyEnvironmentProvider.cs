﻿// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.VB.Visitors;
using Mono.Cecil;

namespace ICSharpCode.ILSpy.VB
{
	public class ILSpyEnvironmentProvider : IEnvironmentProvider
	{
		public string RootNamespace {
			get {
				return "";
			}
		}
		
		public string GetTypeNameForAttribute(ICSharpCode.NRefactory.CSharp.Attribute attribute)
		{
			return attribute.Type.Annotations
				.OfType<Mono.Cecil.MemberReference>()
				.First()
				.FullName;
		}
		
		public ClassType GetClassTypeForAstType(ICSharpCode.NRefactory.CSharp.AstType type)
		{
			var definition = type.Annotations.OfType<TypeReference>().First().ResolveOrThrow();
			
			if (definition.IsClass)
				return ClassType.Class;
			if (definition.IsInterface)
				return ClassType.Interface;
			if (definition.IsEnum)
				return ClassType.Enum;
			if (definition.IsFunctionPointer)
				return ClassType.Delegate;
			if (definition.IsValueType)
				return ClassType.Struct;
			
			return ClassType.Module;
		}
		
		public IType ResolveExpression(ICSharpCode.NRefactory.CSharp.Expression expression)
		{
			throw new NotImplementedException();
		}
	}
}
