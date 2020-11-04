namespace Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using ConfigureAwaitEnforcer;
    using ConfigureAwaitEnforcer.Analyser;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using Shouldly;
    using Xunit;

    public class Tests
    {
        private const string ExampleClassName = "ExampleClass.cs";
        private const string ProjectName = "Tests";
        private readonly DocumentAnalyser _analyser = new DocumentAnalyser();
        private readonly Solution _solution;

        public Tests()
        {
            var mscorelib = Assembly.GetAssembly(typeof(object));

            var msWorkspace = new AdhocWorkspace();
            var projectInfo = ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                ProjectName,
                "TestAssembly",
                LanguageNames.CSharp,
                metadataReferences: new[]
                {
                    MetadataReference.CreateFromFile(mscorelib.Location)
                });

            _solution = msWorkspace.CurrentSolution.AddProject(projectInfo);
        }

        [Fact]
        public void Does_Not_Report_Await_With_ConfigureAwaitFalse()
        {
            InvalidAwaitsForStatement(
                    "await MethodAsync().ConfigureAwait(false);"
                )
                .ShouldBeEmpty();
        }

        [Fact]
        public void Reports_Await_On_Task_With_No_ConfigureAwait()
        {
            InvalidAwaitsForStatement(
                    "await MethodAsync();"
                )
                .Count().ShouldBe(1);
        }

        [Fact]
        public void Reports_Correct_Metadata()
        {
            var invalidAwait = GetInvalidAwaitsForCode(@"
                namespace Tests
                {
                    using System.Threading.Tasks;

                    public class TestClass
                    {
                        public async Task TestMethod()
                        {
                            await MethodAsync(); 
                        }

                        private Task MethodAsync() => Task.CompletedTask;
                    }
                }")
                .Single();

            invalidAwait.FileName.ShouldBe(ExampleClassName);
            invalidAwait.LineNumber.ShouldBe(10);
            invalidAwait.LineText.ShouldBe("await MethodAsync();");
        }

        [Fact]
        public void Does_Not_Report_Call_That_Blocks_With_Wait()
        {
            InvalidAwaitsForStatement(
                    "MethodAsync().Wait();"
                )
                .ShouldBeEmpty();
        }

        [Fact]
        public void Does_Not_Report_Assignment_With_ConfigureAwait()
        {
            InvalidAwaitsForStatement(
                    "var a = await MethodAsyncWithReturn().ConfigureAwait(false);"
                )
                .ShouldBeEmpty();
        }

        [Fact]
        public void Does_Not_Report_Assignment_That_Blocks_With_Result()
        {
            InvalidAwaitsForStatement(
                    "var b = MethodAsyncWithReturn().Result;"
                )
                .ShouldBeEmpty();
        }

        [Fact]
        public void Does_Not_Report_When_ConfigureAwait_Used_Inside_Task_Run()
        {
            InvalidAwaitsForStatement(
                    @"Task.Run(
                        async () => await MethodAsync().ConfigureAwait(false)
                      ).Wait();"
                )
                .ShouldBeEmpty();
        }

        [Fact]
        public void Reports_When_ConfigureAwait_Not_Used_Inside_Task_Run()
        {
            InvalidAwaitsForStatement(
                    @"await Task.Run(
                        async () => await MethodAsync()
                      ).ConfigureAwait(false);"
                )
                .Count().ShouldBe(1);
        }

        [Fact]
        public void Reports_When_Task_Run_Does_Not_Use_ConfigureAwait()
        {
            InvalidAwaitsForStatement(
                    @"await Task.Run(
                        async () => await MethodAsync().ConfigureAwait(false)
                      );"
                )
                .Count().ShouldBe(1);
        }

        [Fact]
        public void Reports_Only_Once_When_Line_Requires_Two_ConfigureAwaits()
        {
            InvalidAwaitsForStatement("await Task.Run(async () => await MethodAsync());")
                .Count()
                .ShouldBe(1);
        }

        [Fact]
        public void Reports_Twice_When_Statement_Requires_Two_ConfigureAwaits_But_Spans_Lines()
        {
            InvalidAwaitsForStatement(
                    @"await Task.Run(
                        async () => await MethodAsync()
                      );")
                .Count().ShouldBe(2);
        }

        [Fact]
        public void Does_Not_Flag_Await_On_Dynamic_With_ConfigureAwait()
        {
            var invalidAwait = GetInvalidAwaitsForCode(@"
                namespace Tests
                {
                    using System.Threading.Tasks;

                    public class TestClass
                    {
                        public async Task TestMethod()
                        {
                            await MethodAsync().ConfigureAwait(false); 
                        }

                        private dynamic MethodAsync() => Task.CompletedTask;
                    }
                }");

            invalidAwait.ShouldBeEmpty();
        }

        [Fact]
        public void Flags_Await_On_Dynamic_Without_ConfigureAwait()
        {
            var invalidAwait = GetInvalidAwaitsForCode(@"
                namespace Tests
                {
                    using System.Threading.Tasks;

                    public class TestClass
                    {
                        public async Task TestMethod()
                        {
                            await MethodAsync(); 
                        }

                        private dynamic MethodAsync() => Task.CompletedTask;
                    }
                }");

            invalidAwait.Count().ShouldBe(1);
        }

        [Fact]
        public void Does_Not_Flag_Await_On_Dynamic_That_Blocks_With_Wait()
        {
            var invalidAwait = GetInvalidAwaitsForCode(@"
                namespace Tests
                {
                    using System.Threading.Tasks;

                    public class TestClass
                    {
                        public async Task TestMethod()
                        {
                            MethodAsync().Wait(); 
                        }

                        private dynamic MethodAsync() => Task.CompletedTask;
                    }
                }");

            invalidAwait.ShouldBeEmpty();
        }

        [Fact]
        public void Does_Not_Flag_Await_On_Dynamic_That_Blocks_With_Result()
        {
            var invalidAwait = GetInvalidAwaitsForCode(@"
                namespace Tests
                {
                    using System.Threading.Tasks;

                    public class TestClass
                    {
                        public async Task TestMethod()
                        {
                            var a = MethodAsync().Result; 
                        }

                        private dynamic MethodAsync() => Task.FromResult(false);
                    }
                }");

            invalidAwait.ShouldBeEmpty();
        }


        private IEnumerable<InvalidAwait> InvalidAwaitsForStatement(string code)
        {
            // Wrap the statement in a class, namespace etc.
            var wrappedCode = @"
                namespace Tests
                {
                    using System.Threading.Tasks;

                    public class TestClass
                    {
                        public async Task TestMethod()
                        {
                            " + code + @"
                        }

                        private Task MethodAsync() => Task.CompletedTask;
                        private Task<int> MethodAsyncWithReturn() => Task.FromResult(1);
                    }
                }";
            return GetInvalidAwaitsForCode(wrappedCode);
        }

        private IEnumerable<InvalidAwait> GetInvalidAwaitsForCode(string code)
        {
            var doc = _solution.Projects.First().AddDocument(ExampleClassName, SourceText.From(code));

            return _analyser.GetInvalidAwaitCallsAsync(doc).Result.ToList();
        }
    }
}
