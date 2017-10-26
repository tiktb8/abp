﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TestApp;
using Volo.Abp.TestApp.Domain;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Repositories
{
    public class MultiTenant_Filter_Tests : EntityFrameworkCoreTestBase
    {
        private ICurrentTenant _fakeCurrentTenant;
        private readonly IRepository<Person> _personRepository;

        public MultiTenant_Filter_Tests()
        {
            _personRepository = GetRequiredService<IRepository<Person>>();
        }

        protected override void AfterAddApplication(IServiceCollection services)
        {
            _fakeCurrentTenant = Substitute.For<ICurrentTenant>();
            services.AddSingleton(_fakeCurrentTenant);
        }

        [Fact]
        public async Task Should_Get_Person_For_Current_Tenant()
        {
            //TenantId = null

            _fakeCurrentTenant.Id.Returns((Guid?)null);

            var people = await _personRepository.GetListAsync();
            people.Count.ShouldBe(1);
            people.Any(p => p.Name == "Douglas").ShouldBeTrue();

            //TenantId = TestDataBuilder.TenantId1

            _fakeCurrentTenant.Id.Returns(TestDataBuilder.TenantId1);

            people = await _personRepository.GetListAsync();
            people.Count.ShouldBe(2);
            people.Any(p => p.Name == TestDataBuilder.TenantId1 + "-Person1").ShouldBeTrue();
            people.Any(p => p.Name == TestDataBuilder.TenantId1 + "-Person2").ShouldBeTrue();

            //TenantId = TestDataBuilder.TenantId2

            _fakeCurrentTenant.Id.Returns(TestDataBuilder.TenantId2);

            people = await _personRepository.GetListAsync();
            people.Count.ShouldBe(0);
        }
    }
}