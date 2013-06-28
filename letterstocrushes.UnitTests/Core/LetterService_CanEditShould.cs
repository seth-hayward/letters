using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using letterstocrushes.Core.Model;
using letterstocrushes.Core.Services;
using letterstocrushes.Core.Interfaces;
using Moq;

namespace letterstocrushes.UnitTests.Core
{
    [TestClass]
    public class LetterService_CanEditShould
    {
        [TestMethod]
        public void ReturnFalseIfCookieValueIsNull()
        {

            // Arrange
            Mock<LetterService> mockLetterService = new Mock<LetterService>(); 

            // Part of arrange -- set up the expectation that the AddLetter 
            // method will be called.
            //mockQueryLetters.Setup(x => x.can);

            mockLetterService.Setup(x => x.canEdit(It.IsAny<string>(), true)).Returns(true);

            // Act

            // Assert
            mockLetterService.VerifyAll();

        }
    }
}
