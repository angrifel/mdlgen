﻿//  This file is part of genmdl - A Source code generator for model definitions.
//  Copyright (c) angrifel

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

namespace ModelGenerator.CSharp.Services
{
  using Model;
  using System;
  using System.Collections.Generic;
  using System.IO;

  public class CSharpGenerator : IGenerator
  {
    private readonly SpecAnalyzer _specAnalizer;

    private readonly ICSharpEntityGenerator _entityGenerator;

    public CSharpGenerator(SpecAnalyzer specAnalizer, ICSharpEntityGeneratorFactory csharpEntityGeneratorFactory)
    {
      if (specAnalizer == null) throw new ArgumentNullException(nameof(specAnalizer));
      if (csharpEntityGeneratorFactory == null) throw new ArgumentNullException(nameof(csharpEntityGeneratorFactory));
      _specAnalizer = specAnalizer;
      _entityGenerator = csharpEntityGeneratorFactory.CreateCSharpEntityGenerator(specAnalizer);
    }

    public IEnumerable<GeneratorOutput> GenerateOutputs()
    {
      var targetInfo = _specAnalizer.Spec.Targets[Constants.CSharpTarget];
      var result = new GeneratorOutput[_specAnalizer.Spec.Enums.Count + _specAnalizer.Spec.Entities.Count];
      var index = 0;
      foreach (var @enum in _specAnalizer.Spec.Enums)
      {
        result[index++] = 
          new GeneratorOutput
          {
            Path = Path.Combine(targetInfo.Path, Path.ChangeExtension(GetFilename(@enum.Key), Constants.CSharpExtension)),
            GenerationRoot = GenerateEnum(targetInfo.Namespace, @enum.Key, @enum.Value)
          };
      }

      foreach (var entity in _specAnalizer.Spec.Entities)
      {
        result[index++] =
          new GeneratorOutput
          {
            Path = Path.Combine(targetInfo.Path, Path.ChangeExtension(GetFilename(entity.Key), Constants.CSharpExtension)),
            GenerationRoot = GenerateEntity(entity.Key, entity.Value.Members)
          };
      }

      return result;
    }

    private static IGenerationRoot GenerateEnum(string @namespace, string enumName, IList<EnumMember> enumMembers)
    {
      var members = new List<CSharpEnumMember>(enumMembers.Count);
      foreach (var member in enumMembers)
      {
        members.Add(GenerateEnumMember(member));
      }

      return new CSharpNamespace
      {
        Name = @namespace,
        Types = new List<CSharpType>
        {
          new CSharpEnum
          {
            Name = SpecFunctions.ToPascalCase(enumName),
            Members = members
          }
        }
      };
    }

    private static CSharpEnumMember GenerateEnumMember(EnumMember member)
    {
      return new CSharpEnumMember
      {
        Name = SpecFunctions.ToPascalCase(member.Name),
        Value = member.Value
      };
    }

    private IGenerationRoot GenerateEntity(string entityName, IDictionary<string, IEntityMemberInfo> entityMembers)
    {
      return new CSharpNamespace
      {
        Name = _specAnalizer.Spec.Targets[Constants.CSharpTarget].Namespace,
        Types = new List<CSharpType>
        {
          _entityGenerator.GenerateEntity(entityName, entityMembers)
        }
      };
    }

    private static string GetFilename(string type) => SpecFunctions.ToPascalCase(type);
  }
}