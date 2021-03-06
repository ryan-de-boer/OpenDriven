// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Engine.Internal;
using NUnit.Engine.Tests.Services.TestRunnerFactoryTests.Results;
using NUnit.Framework;

namespace NUnit.Engine.Tests.Services.TestRunnerFactoryTests.TestCases
{
#if !NETCOREAPP
    internal static class Net20AssemblyTestCases
    {
        public static IEnumerable<TestCaseData> TestCases
        {
            get
            {
                foreach (var processModel in Enum.GetValues(typeof(ProcessModel)).Cast<ProcessModel>())
                {
                    foreach (var domainUsage in Enum.GetValues(typeof(DomainUsage)).Cast<DomainUsage>())
                    {
                        yield return SingleAssemblyStringCtorTest(processModel, domainUsage);
                        yield return SingleAssemblyListCtorTest(processModel, domainUsage);
                        yield return SingleUnknownExtensionTest(processModel, domainUsage);
                        yield return TwoAssembliesTest(processModel, domainUsage);
                        yield return TwoUnknownsTest(processModel, domainUsage);
                    }
                }
            }
        }

        private static TestCaseData SingleAssemblyStringCtorTest(ProcessModel processModel, DomainUsage domainUsage)
        {
            var testName = "Single assembly (string ctor) - " +
                           $"{nameof(EnginePackageSettings.ProcessModel)}:{processModel} " +
                           $"{nameof(EnginePackageSettings.DomainUsage)}:{domainUsage}";

            var package = TestPackageFactory.OneAssemblyStringCtor();
            package.AddSetting(EnginePackageSettings.ProcessModel, processModel.ToString());
            package.AddSetting(EnginePackageSettings.DomainUsage, domainUsage.ToString());

            var expected = Net20SingleAssemblyStringCtorExpectedRunnerResults.ResultFor(processModel, domainUsage);
            var testCase = new TestCaseData(package, expected).SetName($"{{m}}({testName})");
            return testCase;
        }

        private static TestCaseData SingleAssemblyListCtorTest(ProcessModel processModel, DomainUsage domainUsage)
        {
            var testName = "Single assembly (list ctor) - " +
                              $"{nameof(EnginePackageSettings.ProcessModel)}:{processModel} " +
                              $"{nameof(EnginePackageSettings.DomainUsage)}:{domainUsage}";

            var package = TestPackageFactory.OneAssemblyListCtor();
            package.AddSetting(EnginePackageSettings.ProcessModel, processModel.ToString());
            package.AddSetting(EnginePackageSettings.DomainUsage, domainUsage.ToString());

            var expected = Net20SingleAssemblyListCtorExpectedRunnerResults.ResultFor(processModel, domainUsage);
            return new TestCaseData(package, expected).SetName($"{{m}}({testName})");
        }

        private static TestCaseData SingleUnknownExtensionTest(ProcessModel processModel, DomainUsage domainUsage)
        {
            var testName = "Single unknown - " +
                           $"{nameof(EnginePackageSettings.ProcessModel)}:{processModel} " +
                           $"{nameof(EnginePackageSettings.DomainUsage)}:{domainUsage}";

            var package = TestPackageFactory.OneUnknownExtension();
            package.AddSetting(EnginePackageSettings.ProcessModel, processModel.ToString());
            package.AddSetting(EnginePackageSettings.DomainUsage, domainUsage.ToString());

            var expected = Net20SingleAssemblyListCtorExpectedRunnerResults.ResultFor(processModel, domainUsage);
            return new TestCaseData(package, expected).SetName($"{{m}}({testName})");
        }

        private static TestCaseData TwoAssembliesTest(ProcessModel processModel, DomainUsage domainUsage)
        {
            var testName = "Two assemblies - " +
                           $"{nameof(EnginePackageSettings.ProcessModel)}:{processModel} " +
                           $"{nameof(EnginePackageSettings.DomainUsage)}:{domainUsage}";

            var package = TestPackageFactory.TwoAssemblies();
            package.AddSetting(EnginePackageSettings.ProcessModel, processModel.ToString());
            package.AddSetting(EnginePackageSettings.DomainUsage, domainUsage.ToString());

            var expected = Net20TwoAssemblyExpectedRunnerResults.ResultFor(processModel, domainUsage);
            return new TestCaseData(package, expected).SetName($"{{m}}({testName})");
        }

        private static TestCaseData TwoUnknownsTest(ProcessModel processModel, DomainUsage domainUsage)
        {
            var testName = "Two unknown extensions - " +
                           $"{nameof(EnginePackageSettings.ProcessModel)}:{processModel} " +
                           $"{nameof(EnginePackageSettings.DomainUsage)}:{domainUsage}";

            var package = TestPackageFactory.TwoUnknownExtension();
            package.AddSetting(EnginePackageSettings.ProcessModel, processModel.ToString());
            package.AddSetting(EnginePackageSettings.DomainUsage, domainUsage.ToString());

            var expected = Net20TwoAssemblyExpectedRunnerResults.ResultFor(processModel, domainUsage);
            return new TestCaseData(package, expected).SetName($"{{m}}({testName})");
        }
    }
#endif
}