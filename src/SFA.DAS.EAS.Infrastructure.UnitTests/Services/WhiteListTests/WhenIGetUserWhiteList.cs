﻿using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Models.WhileList;
using SFA.DAS.EmployerApprenticeshipsService.Infrastructure.Caching;
using SFA.DAS.EmployerApprenticeshipsService.Infrastructure.Services;

namespace SFA.DAS.EAS.Infrastructure.UnitTests.Services.WhiteListTests
{
    class WhenIGetUserWhiteList
    {
        private Mock<ICacheProvider> _cacheProvider;
        private Mock<UserWhiteListFileBasedService> _mockUserWhiteList;
        private UserWhiteListFileBasedService _userWhiteList;
        private UserWhiteListLookUp _lookup;

        [SetUp]
        public void Arrange()
        {
            _cacheProvider = new Mock<ICacheProvider>();
            _mockUserWhiteList = new Mock<UserWhiteListFileBasedService>(_cacheProvider.Object);
            _userWhiteList = _mockUserWhiteList.Object;

            _lookup = new UserWhiteListLookUp
            {
                EmailPatterns = new List<string>
                {
                    "[a-zA-Z0-9.-]*@test.com"
                }
            };
        }

        [Test]
        public void ThenIShouldGetWhiteListedUserEmails()
        {
            //Assign
            _mockUserWhiteList.Setup(x => x.ReadFileByIdSync<UserWhiteListLookUp>(It.IsAny<string>()))
                .Returns(_lookup);

            _cacheProvider.Setup(x => x.Get<UserWhiteListLookUp>(It.IsAny<string>()))
                          .Returns((UserWhiteListLookUp) null);

            //Act
            var result = _userWhiteList.IsEmailOnWhiteList("test@test.com");

            //Assert
            _mockUserWhiteList.Verify(x => x.ReadFileByIdSync<UserWhiteListLookUp>("user_white_list"), Times.Once);
            Assert.IsTrue(result);
        }

        [Test]
        public void ThenIShouldReadFromCacheIfUserWhiteListHasBeenCached()
        {
            //Assign
            _cacheProvider.Setup(x => x.Get<UserWhiteListLookUp>(It.IsAny<string>())).Returns(_lookup);
            _mockUserWhiteList.Setup(x => x.ReadFileByIdSync<UserWhiteListLookUp>(It.IsAny<string>()));

            //Act
            var result = _userWhiteList.IsEmailOnWhiteList("test@test.com");

            //Assert
            Assert.IsTrue(result);
            _cacheProvider.Verify(x => x.Get<UserWhiteListLookUp>(nameof(UserWhiteListLookUp)), Times.Once);
            _mockUserWhiteList.Verify(x => x.ReadFileByIdSync<UserWhiteListLookUp>(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ThenIfIPassAnEmaptyEmailTheUserWhiteListShouldNotBeLookedUp()
        {
            //Assign
            _cacheProvider.Setup(x => x.Get<UserWhiteListLookUp>(It.IsAny<string>()));
            _mockUserWhiteList.Setup(x => x.ReadFileByIdSync<UserWhiteListLookUp>(It.IsAny<string>()));

            //Act
            _userWhiteList.IsEmailOnWhiteList(string.Empty);

            //Assert
            _cacheProvider.Verify(x => x.Get<UserWhiteListLookUp>(It.IsAny<string>()), Times.Never);
            _mockUserWhiteList.Verify(x => x.ReadFileByIdSync<UserWhiteListLookUp>(It.IsAny<string>()), Times.Never);
        }
    }
}
