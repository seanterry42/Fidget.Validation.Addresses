﻿using System;
using System.Collections.Generic;
using System.Text;
using Fidget.Validation.Addresses.Service.Metadata;
using System.Linq;

namespace Fidget.Validation.Addresses.Validation
{
    /// <summary>
    /// Validator that ensures required elements are present in the address.
    /// </summary>

    class RequiredElementsValidator : IAddressValidator
    {
        /// <summary>
        /// Collection of failures indexed by field type.
        /// </summary>
        
        internal static readonly IReadOnlyDictionary<AddressField,ValidationFailure> Failures = Enum.GetValues( typeof(AddressField) )
            .OfType<AddressField>()
            .Select( _=> new ValidationFailure( _, AddressFieldError.MissingRequiredField ) )
            .ToDictionary( _=> _.Field );

        /// <summary>
        /// Validates the given address.
        /// </summary>
        
        public IEnumerable<ValidationFailure> Validate( AddressData address, IGlobalMetadata global, ICountryMetadata country, IProvinceMetadata province, ILocalityMetadata locality, ISublocalityMetadata sublocality )
        {
            if ( address == null ) throw new ArgumentNullException( nameof( address ) );
            if ( global == null ) throw new ArgumentNullException( nameof( global ) );

            var failures = new List<ValidationFailure>();
            var defaults = new AddressField[] { AddressField.Country };
            var required = defaults
                .Union( country?.Required ?? defaults )
                .Union( province?.Required ?? defaults )
                .Union( locality?.Required ?? defaults )
                .Union( sublocality?.Required ?? defaults )
                .Distinct();
            
            void validate( AddressField field, string value ) 
            { 
                if ( string.IsNullOrWhiteSpace( value ) && required.Contains( field ) ) 
                    failures.Add( Failures[field] ); 
            }

            validate( AddressField.Country, address.Country );
            validate( AddressField.Province, address.Province );
            validate( AddressField.Locality, address.Locality );
            validate( AddressField.Sublocality, address.Sublocality );
            validate( AddressField.PostalCode, address.PostalCode );
            validate( AddressField.SortingCode, address.SortingCode );
            validate( AddressField.StreetAddress, address.StreetAddress );
            validate( AddressField.Organization, address.Organization );
            validate( AddressField.Name, address.Name );
            
            return failures;
        }
    }
}