﻿using Fidget.Commander;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fidget.Validation.Addresses.Metadata.Commands
{
    /// <summary>
    /// Command for querying country metadata.
    /// </summary>

    public struct CountryMetadataQuery : ICommand<CountryMetadata>
    {
        /// <summary>
        /// Gets or sets the key of the country to return.
        /// Due to names not being present on global metadata, only the country key may be searched.
        /// </summary>
        
        public string Country { get; set; }
        
        /// <summary>
        /// When specified, requests metadata be in the specified language.
        /// Available languages for metadata are defined in the default entry for the country.
        /// </summary>
        
        public string Language { get; set; }

        /// <summary>
        /// Creates and returns a query to return the country for the given address entry.
        /// </summary>
        /// <param name="address">Address for which to create a query.</param>
        
        public static CountryMetadataQuery For( AddressData address )
        {
            if ( address == null ) throw new ArgumentNullException( nameof( address ) );

            return new CountryMetadataQuery
            {
                Country = address.Country,
                Language = address.Language,
            };
        }

        /// <summary>
        /// Handler for the command.
        /// </summary>

        public class Handler : ICommandHandler<CountryMetadataQuery, CountryMetadata>
        {
            /// <summary>
            /// Query context.
            /// </summary>
            
            readonly IMetadataQueryContext Context;

            /// <summary>
            /// Constructs a handler for the command.
            /// </summary>
            /// <param name="context">Query context.</param>
            
            public Handler( IMetadataQueryContext context )
            {
                Context = context ?? throw new ArgumentNullException( nameof(context) );
            }

            /// <summary>
            /// Executes the command.
            /// </summary>
            
            public async Task<CountryMetadata> Handle( CountryMetadataQuery command, CancellationToken cancellationToken )
            {
                CountryMetadata result = null;

                if ( await Context.Dispatcher.Execute( GlobalMetadataQuery.Default, cancellationToken ) is GlobalMetadata global )
                if ( Context.Builder.GetChildKey( global, command.Country ) is string key )
                if ( Context.Builder.BuildIdentifier( global, key, command.Language ) is string id )
                {
                    result = await Context.Client.Query<CountryMetadata>( id );
                }
                
               return result;;
            }
        }
    }
}