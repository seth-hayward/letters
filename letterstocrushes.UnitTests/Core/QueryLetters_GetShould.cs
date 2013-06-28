using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using letterstocrushes.Core.Model;
using letterstocrushes.Core.Services;
using letterstocrushes.Core.Interfaces;
using Moq;

namespace letterstocrushes.UnitTests.Core
{
    [TestClass]
    public class QueryLetters_GetShould
    {
        [TestMethod]
        public void GetLetter()
        {

            // Arrange
            Mock<IQueryLetters> mockQueryLetters = new Mock<IQueryLetters>();
            Mock<MailService> mockMailService = new Mock<MailService>();

            Mock<IQueryBookmarks> mockQueryBookmarks = new Mock<IQueryBookmarks>();

            BookmarkService mockBookmarkService = new BookmarkService(mockQueryBookmarks.Object);

            LetterService myLetterService = new LetterService(mockQueryLetters.Object, mockMailService.Object, mockBookmarkService);

            // Part of arrange -- set up the expectation that the AddLetter 
            // method will be called.
            mockQueryLetters.Setup(x => x.getLetter(It.IsAny<int>()));

            // Act
            myLetterService.getLetter(10000);

            // Assert
            mockQueryLetters.VerifyAll();
        }

    }
}
