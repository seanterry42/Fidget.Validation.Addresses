﻿using Fidget.Validation.Addresses.Service;
using Fidget.Validation.Addresses.Service.Metadata;
using Fidget.Validation.Addresses.Service.Metadata.Internal;
using Fidget.Validation.Addresses.Validation;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fidget.Validation.Addresses
{
    public class AddressServiceTests
    {
        Mock<IServiceClient> MockClient = new Mock<IServiceClient>();
        Mock<IValidationContextFactory> MockFactory = new Mock<IValidationContextFactory>();
        IServiceClient client => MockClient?.Object;
        IValidationContextFactory factory => MockFactory?.Object;
        
        IAddressService instance => new AddressService.Implementation( client, factory );

        public class Constructor : AddressServiceTests
        {
            [Fact]
            public void Requires_client()
            {
                MockClient = null;
                Assert.Throws<ArgumentNullException>( nameof(client), ()=>instance );
            }

            [Fact]
            public void Requires_factory()
            {
                MockFactory = null;
                Assert.Throws<ArgumentNullException>( nameof(factory), ()=>instance );
            }
        }

        public class GetGlobalAsync : AddressServiceTests
        {
            /// <summary>
            /// Client response setup.
            /// </summary>

            public static IEnumerable<object[]> GetGlobalResponses => new object[][]
            {
                new object[] { new GlobalMetadata { Id = "data" } },
                new object[] { default( GlobalMetadata ) },
            };

            [Theory]
            [MemberData( nameof( GetGlobalResponses ) )]
            public async Task Returns_valueFromClient( IGlobalMetadata expected )
            {
                MockClient.Setup( _ => _.Query<GlobalMetadata>( "data" ) ).ReturnsAsync( (GlobalMetadata)expected ).Verifiable();
                var actual = await instance.GetGlobalAsync();

                Assert.Equal( expected, actual );
                MockClient.VerifyAll();
            }
        }

        public class GetCountryAsync : AddressServiceTests
        {
            string countryKey;
            string language;
            CountryMetadata defaultCountry = new CountryMetadata
            {
                Id = "data/ZZ",
                Format = "%N%n%O%n%A%n%C",
                Required = new AddressField[] { AddressField.StreetAddress, AddressField.Locality },
                Uppercase = new AddressField[] { AddressField.Locality },
                StateType = "province",
                LocalityType = "city",
                SublocalityType = "suburb",
                PostalCodeType = "postal",
            };

            async Task<ICountryMetadata> invoke() => await instance.GetCountryAsync( countryKey, language );

            [Fact]
            public async Task Requires_countryKey()
            {
                countryKey = null;
                language = null;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( countryKey ), invoke );
            }

            /// <summary>
            /// Country scenarios.
            /// </summary>

            public static IEnumerable<object[]> GetCountryValues => new object[][]
            {
                new object[] { "XX", null, "data/XX", new CountryMetadata { Id = "data/XX" } },
                new object[] { "XX", null, "data/XX", null },
                new object[] { "XX", "abc", "data/XX--abc", new CountryMetadata { Id = "data/XX--abc" } },
                new object[] { "XX", "abc", "data/XX--abc", null },
            };

            [Theory]
            [MemberData( nameof( GetCountryValues ) )]
            public async Task Returns_valueFromServiceClient( string countryKey, string language, string id, ICountryMetadata expected )
            {
                MockClient.Setup( _ => _.Query<CountryMetadata>( defaultCountry.Id ) ).ReturnsAsync( defaultCountry ).Verifiable();
                MockClient.Setup( _ => _.Query<CountryMetadata>( id ) ).ReturnsAsync( (CountryMetadata)expected ).Verifiable();
                
                var actual = await instance.GetCountryAsync( countryKey, language );
                Assert.Equal( expected, actual );
                MockClient.VerifyAll();
            }

            public static IEnumerable<object[]> GetFilledCountryValues => new object[][]
            {
                new object[] { "XX", new CountryMetadata { Id = "data/XX", Format = Guid.NewGuid().ToString() } },
                new object[] { "XX", new CountryMetadata { Id = "data/XX", Required = new AddressField[] { AddressField.StreetAddress, AddressField.Locality, AddressField.Province } } },
                new object[] { "XX", new CountryMetadata { Id = "data/XX", Uppercase = new AddressField[] { AddressField.Locality, AddressField.Sublocality } } },
                new object[] { "XX", new CountryMetadata { Id = "data/XX", StateType = Guid.NewGuid().ToString() } },
                new object[] { "XX", new CountryMetadata { Id = "data/XX", LocalityType = Guid.NewGuid().ToString() } },
                new object[] { "XX", new CountryMetadata { Id = "data/XX", SublocalityType = Guid.NewGuid().ToString() } },
                new object[] { "XX", new CountryMetadata { Id = "data/XX", PostalCodeType = Guid.NewGuid().ToString() } },
                new object[] { "XX", new CountryMetadata { Id = "data/XX" } },
            };

            /// <summary>
            /// Property values from the default country should be present on the result if those values are null from the service.
            /// </summary>

            [Theory]
            [MemberData( nameof( GetFilledCountryValues ) )]
            public async Task Returns_defaultValues_whenResultValuesNull( string countryKey, ICountryMetadata country )
            {
                MockClient.Setup( _ => _.Query<CountryMetadata>( defaultCountry.Id ) ).ReturnsAsync( defaultCountry ).Verifiable();
                MockClient.Setup( _ => _.Query<CountryMetadata>( country.Id ) ).ReturnsAsync( (CountryMetadata)country ).Verifiable();

                var format = country.Format ?? defaultCountry.Format;
                var required = country.Required ?? defaultCountry.Required;
                var uppercase = country.Uppercase ?? defaultCountry.Uppercase;
                var stateType = country.StateType ?? defaultCountry.StateType;
                var localityType = country.LocalityType ?? defaultCountry.LocalityType;
                var sublocalityType = country.SublocalityType ?? defaultCountry.SublocalityType;
                var postalCodeType = country.PostalCodeType ?? defaultCountry.PostalCodeType;

                var actual = await instance.GetCountryAsync( countryKey, language );
                Assert.Equal( format, country.Format );
                Assert.Equal( required, country.Required );
                Assert.Equal( uppercase, country.Uppercase );
                Assert.Equal( stateType, country.StateType );
                Assert.Equal( localityType, country.LocalityType );
                Assert.Equal( sublocalityType, country.SublocalityType );
                Assert.Equal( postalCodeType, country.PostalCodeType );

                MockClient.VerifyAll();
            }
        }

        public class GetProvinceAsync : AddressServiceTests
        {
            string countryKey = Guid.NewGuid().ToString();
            string provinceKey = Guid.NewGuid().ToString();
            string language = Guid.NewGuid().ToString();
            async Task<IProvinceMetadata> invoke() => await instance.GetProvinceAsync( countryKey, provinceKey, language );

            [Fact]
            public async Task Requires_countryKey()
            {
                countryKey = null;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( countryKey ), invoke );
            }

            [Fact]
            public async Task Requires_provinceKey()
            {
                provinceKey = null;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( provinceKey ), invoke );
            }

            public static IEnumerable<object[]> GetArguments() => new object[][]
            {
                new object[] { "XX", "ZZ", null, "data/XX/ZZ", null },
                new object[] { "XX", "ZZ", null, "data/XX/ZZ", new ProvinceMetadata { Id = "data/XX/ZZ" } },
                new object[] { "XX", "ZZ", "xyz", "data/XX/ZZ--xyz", null },
                new object[] { "XX", "ZZ", "xyz", "data/XX/ZZ--xyz", new ProvinceMetadata { Id = "data/XX/ZZ--xyz" } },
            };

            [Theory]
            [MemberData( nameof( GetArguments ) )]
            public async Task Returns_clientResult( string countryKey, string provinceKey, string language, string id, IProvinceMetadata expected )
            {
                this.countryKey = countryKey;
                this.provinceKey = provinceKey;
                this.language = language;
                MockClient.Setup( _ => _.Query<ProvinceMetadata>( id ) ).ReturnsAsync( (ProvinceMetadata)expected ).Verifiable();

                var actual = await invoke();
                Assert.Equal( expected, actual );
                MockClient.VerifyAll();
            }
        }

        public class GetLocalityAsync : AddressServiceTests
        {
            string countryKey = Guid.NewGuid().ToString();
            string provinceKey = Guid.NewGuid().ToString();
            string localityKey = Guid.NewGuid().ToString();
            string language = Guid.NewGuid().ToString();
            async Task<ILocalityMetadata> invoke() => await instance.GetLocalityAsync( countryKey, provinceKey, localityKey, language );

            [Fact]
            public async Task Requires_countryKey()
            {
                countryKey = null;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( countryKey ), invoke );
            }

            [Fact]
            public async Task Requires_provinceKey()
            {
                provinceKey = null;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( provinceKey ), invoke );
            }

            [Fact]
            public async Task Requires_localityKey()
            {
                localityKey = null;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( localityKey ), invoke );
            }

            public static IEnumerable<object[]> GetArguments() => new object[][]
            {
                new object[] { "XX", "ZZ", "ZY", null, "data/XX/ZZ/ZY", null },
                new object[] { "XX", "ZZ", "ZY", null, "data/XX/ZZ/ZY", new LocalityMetadata { Id = "data/XX/ZZ/ZY" } },
                new object[] { "XX", "ZZ", "ZY", "xyz", "data/XX/ZZ/ZY--xyz", null },
                new object[] { "XX", "ZZ", "ZY", "xyz", "data/XX/ZZ/ZY--xyz", new LocalityMetadata { Id = "data/XX/ZZ/ZY--xyz" } },
            };

            [Theory]
            [MemberData( nameof( GetArguments ) )]
            public async Task Returns_clientResult( string countryKey, string provinceKey, string localityKey, string language, string id, ILocalityMetadata expected )
            {
                this.countryKey = countryKey;
                this.provinceKey = provinceKey;
                this.localityKey = localityKey;
                this.language = language;
                MockClient.Setup( _ => _.Query<LocalityMetadata>( id ) ).ReturnsAsync( (LocalityMetadata)expected ).Verifiable();

                var actual = await invoke();
                Assert.Equal( expected, actual );
                MockClient.VerifyAll();
            }
        }

        public class GetSublocalityAsync : AddressServiceTests
        {
            string countryKey = Guid.NewGuid().ToString();
            string provinceKey = Guid.NewGuid().ToString();
            string localityKey = Guid.NewGuid().ToString();
            string sublocalityKey = Guid.NewGuid().ToString();
            string language = Guid.NewGuid().ToString();
            async Task<ISublocalityMetadata> invoke() => await instance.GetSublocalityAsync( countryKey, provinceKey, localityKey, sublocalityKey, language );

            [Fact]
            public async Task Requires_countryKey()
            {
                countryKey = null;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( countryKey ), invoke );
            }

            [Fact]
            public async Task Requires_provinceKey()
            {
                provinceKey = null;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( provinceKey ), invoke );
            }

            [Fact]
            public async Task Requires_localityKey()
            {
                localityKey = null;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( localityKey ), invoke );
            }

            [Fact]
            public async Task Requires_sublocalityKey()
            {
                sublocalityKey = null;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( sublocalityKey ), invoke );
            }

            public static IEnumerable<object[]> GetArguments() => new object[][]
            {
                new object[] { "XX", "ZZ", "ZY", "XY", null, "data/XX/ZZ/ZY/XY", null },
                new object[] { "XX", "ZZ", "ZY", "XY", null, "data/XX/ZZ/ZY/XY", new SublocalityMetadata { Id = "data/XX/ZZ/ZY/XY" } },
                new object[] { "XX", "ZZ", "ZY", "XY", "xyz", "data/XX/ZZ/ZY/XY--xyz", null },
                new object[] { "XX", "ZZ", "ZY", "XY", "xyz", "data/XX/ZZ/ZY/XY--xyz", new SublocalityMetadata { Id = "data/XX/ZZ/ZY--xyz" } },
            };

            [Theory]
            [MemberData( nameof( GetArguments ) )]
            public async Task Returns_clientResult( string countryKey, string provinceKey, string localityKey, string sublocalityKey, string language, string id, ISublocalityMetadata expected )
            {
                this.countryKey = countryKey;
                this.provinceKey = provinceKey;
                this.localityKey = localityKey;
                this.sublocalityKey = sublocalityKey;
                this.language = language;
                MockClient.Setup( _ => _.Query<SublocalityMetadata>( id ) ).ReturnsAsync( (SublocalityMetadata)expected ).Verifiable();

                var actual = await invoke();
                Assert.Equal( expected, actual );
                MockClient.VerifyAll();
            }
        }

        public class TryGetCountryKey : AddressServiceTests
        {
            public static IEnumerable<object[]> CountryNotInGlobalCases()
            {
                var values = new string[] { null, string.Empty, "XB" };
                var globals = new GlobalMetadata[]
                {
                    null,
                    new GlobalMetadata { Countries = null },
                    new GlobalMetadata { Countries = new string[0] },
                    new GlobalMetadata { Countries = new string[] { "XW", "XA" } },
                };

                return
                    from value in values
                    from global in globals
                    select new object[] { global, value };
            }

            [Theory]
            [MemberData(nameof(CountryNotInGlobalCases))]
            public void WhenCountryNotInGlobal_returns_false( IGlobalMetadata global, string value )
            {
                var actual = instance.TryGetCountryKey( global, value, out string countryKey );
                Assert.False( actual );
                Assert.Null( countryKey );
            }

            [Fact]
            public void WhenCountryInGlobal_returns_true()
            {
                var global = new GlobalMetadata { Countries = new string[] { "XW", "XA" } };
                var value = "XW";
                var actual = instance.TryGetCountryKey( global, value, out string countryKey );

                Assert.True( actual );
                Assert.Equal( value, countryKey );
            }
        }

        public class TryGetChildKey : AddressServiceTests
        {
            public static IEnumerable<object[]> ChildNotInParentCases()
            {
                var values = new string[] { null, string.Empty, "XB" };
                var collections = new IEnumerable<string>[]
                {
                    null,
                    new string[0],
                    new string[] { "XX", "XA" },
                };

                return
                    from keys in collections
                    from value in values
                    select new object[] { new CountryMetadata { ChildRegionKeys = keys, ChildRegionNames = keys, ChildRegionLatinNames = keys }, value };
            }

            [Theory]
            [MemberData(nameof(ChildNotInParentCases))]
            public void WhenChildNotInParent_returns_false( IHierarchicalMetadata parent, string value )
            {
                var actual = instance.TryGetChildKey( parent, value, out string key );
                Assert.False( actual );
                Assert.Null( key );
            }

            [Fact]
            public void WhenChildInKeys_returns_true()
            {
                var parent = new CountryMetadata { ChildRegionKeys = new string[] { "XW", "XA" } };
                var expected = "XA";
                var result = instance.TryGetChildKey( parent, expected, out string actual );

                Assert.True( result );
                Assert.Equal( expected, actual );
            }

            public static IEnumerable<object[]> ChildInParentNamesCases()
            {
                string random() => Guid.NewGuid().ToString();
                var value = random();

                var keys = new string[] { "XX", "XW", "XA" };
                var names = new string[] { value, value.ToLowerInvariant(), value.ToUpperInvariant() };
                var opposing = new string[][] { null, new string[0], new string[] { random(), random(), random() } };

                return
                    from name in names
                    from other in opposing
                    select new object[] { value, keys, new string[] { random(), name, random() }, other };
            }

            [Theory]
            [MemberData(nameof(ChildInParentNamesCases),DisableDiscoveryEnumeration =true)]
            public void WhenChildInNames_returns_true( string value, string[] keys, string[] names, string[] other )
            {
                var parent = new CountryMetadata 
                { 
                    ChildRegionKeys = keys ,
                    ChildRegionNames = names,
                    ChildRegionLatinNames = other,
                };
                 
                var result = instance.TryGetChildKey( parent, value, out string actual );

                Assert.True( result );
                Assert.Equal( "XW", actual );
            }

            [Theory]
            [MemberData( nameof( ChildInParentNamesCases ), DisableDiscoveryEnumeration = true )]
            public void WhenChildInLatinNames_returns_true( string value, string[] keys, string[] names, string[] other )
            {
                var parent = new CountryMetadata
                {
                    ChildRegionKeys = keys,
                    ChildRegionNames = other,
                    ChildRegionLatinNames = names,
                };

                var result = instance.TryGetChildKey( parent, value, out string actual );

                Assert.True( result );
                Assert.Equal( "XW", actual );
            }
        }

        public class ValidateAsync : AddressServiceTests
        {
            AddressData address = new AddressData();
            string language = Guid.NewGuid().ToString();
            async Task<IEnumerable<ValidationFailure>> invoke() => await instance.ValidateAsync( address, language );
            
            [Fact]
            public async Task Requires_address()
            {
                address = null;
                await Assert.ThrowsAsync<ArgumentNullException>( nameof( address ), invoke );
            }

            Mock<IValidationContext> MockContext = new Mock<IValidationContext>();
            IValidationContext context => MockContext?.Object;

            Mock<IAddressValidator> MockValidator1 = new Mock<IAddressValidator>();
            Mock<IAddressValidator> MockValidator2 = new Mock<IAddressValidator>();

            [Fact]
            public async Task Returns_collectionOfFailures()
            {
                var current = instance;
                var expected = new ValidationFailure[]
                {
                    new ValidationFailure( AddressField.PostalCode, AddressFieldError.InvalidFormat ),
                    new ValidationFailure( AddressField.Province, AddressFieldError.UnkownValue ),
                    new ValidationFailure( AddressField.StreetAddress, AddressFieldError.MissingRequiredField ),
                };

                MockFactory.SetupGet( _ => _.Validators ).Returns( new IAddressValidator[] { MockValidator1.Object, MockValidator2.Object } );
                MockFactory.Setup( _ => _.Create( address, current, language ) ).ReturnsAsync( context );
                MockValidator1.Setup( _=> _.Validate( address, context ) ).Returns( expected.Take(2) );
                MockValidator2.Setup( _=> _.Validate( address, context ) ).Returns( expected.Skip(2) );

                var actual = await current.ValidateAsync( address, language );
                Assert.Equal( expected, actual );
            }
        }
    }
}