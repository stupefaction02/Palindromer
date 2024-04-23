namespace Palindromer.Core.Tests
{
    public class PalindromeService
    {
        [Fact]
        public void Variety()
        {
            var service = new Palindromer.Server.Services.PalindromeService();

            string a1 = "madam";
            bool result1 = service.CheckPalindrome(a1);

            string a2 = "madamasdasdasd";
            bool result2 = service.CheckPalindrome(a2);

            string a3 = "aaaaaaaa";
            bool result3 = service.CheckPalindrome(a3);

            string a4 = "aboba";
            bool result4 = service.CheckPalindrome(a4);

            Assert.True(result1);
            Assert.False(result2);
            Assert.True(result3);
            Assert.True(result4);
        }

        [Fact]
        public void One_True()
        {
            var service = new Palindromer.Server.Services.PalindromeService();

            string a1 = "madam";
            bool result1 = service.CheckPalindrome(a1);

            Assert.True(result1);
        }

        [Fact]
        public void One_False()
        {
            var service = new Palindromer.Server.Services.PalindromeService();

            string a1 = "madamsadasasfaf";
            bool result1 = service.CheckPalindrome(a1);

            Assert.False(result1);
        }
    }
}