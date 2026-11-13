using EasyRun.Core;
using Xunit;

namespace EasyRun.Tests
{
    public class AliasNameTests
    {
        [Theory]
        [InlineData("vs")]
        [InlineData("my-tool")]
        [InlineData("tool_2")]
        [InlineData("code.exe")]
        public void Valid_names(string name)
        {
            Assert.Null(AliasName.Validate(name));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("has space")]
        [InlineData("a;b")]
        [InlineData("a/b")]
        [InlineData("a%b")]
        [InlineData(".exe")]
        public void Invalid_names(string name)
        {
            Assert.NotNull(AliasName.Validate(name));
        }

        [Theory]
        [InlineData("code", "code")]
        [InlineData("code.exe", "code")]
        [InlineData("code.EXE", "code")]
        [InlineData("my.tool", "my.tool")]
        public void StripExe(string input, string expected)
        {
            Assert.Equal(expected, AliasName.StripExe(input));
        }

        [Theory]
        [InlineData("code", "code.exe")]
        [InlineData("code.exe", "code.exe")]
        public void ToKey_always_has_exe(string input, string expected)
        {
            Assert.Equal(expected, AliasName.ToKey(input));
        }

        [Fact]
        public void ShortcutFileName_is_safe()
        {
            Assert.Equal("my.tool.lnk", AliasName.ToShortcutFileName("my.tool.exe"));
        }
    }

    public class ExecutableInfoTests
    {
        [Fact]
        public void Detects_console_vs_gui_on_real_binaries()
        {
            string system = System.Environment.SystemDirectory;
            Assert.False(ExecutableInfo.IsGuiProgram(System.IO.Path.Combine(system, "cmd.exe")));
            // notepad.exe moved around across builds; explorer is a stable GUI PE.
            string explorer = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows), "explorer.exe");
            Assert.True(ExecutableInfo.IsGuiProgram(explorer));
        }

        [Theory]
        [InlineData("x.cmd")]
        [InlineData("x.bat")]
        [InlineData("x.ps1")]
        public void Scripts_are_not_gui(string path)
        {
            Assert.False(ExecutableInfo.IsGuiProgram(path));
        }
    }

    public class LooksLikeFullPathTests
    {
        [Theory]
        [InlineData(@"C:\Tools\app.exe", true)]
        [InlineData(@"\\server\share\app.exe", true)]
        [InlineData("app.exe", false)]
        [InlineData("", false)]
        [InlineData("C:app.exe", false)]
        public void Cases(string path, bool expected)
        {
            Assert.Equal(expected, AliasService.LooksLikeFullPath(path));
        }
    }
}
