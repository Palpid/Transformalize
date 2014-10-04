﻿using System;
using System.Threading.Tasks;
using Transformalize.Libs.Elasticsearch.Net.Domain.RequestParameters;
using Transformalize.Libs.Nest.Domain.Responses;
using Transformalize.Libs.Nest.DSL;

namespace Transformalize.Libs.Nest
{
	public partial class ElasticClient
	{
		/// <inheritdoc />
		public IClusterStateResponse ClusterState(Func<ClusterStateDescriptor, ClusterStateDescriptor> clusterStateSelector = null)
		{
			clusterStateSelector = clusterStateSelector ?? (s => s);
			return this.Dispatch<ClusterStateDescriptor, ClusterStateRequestParameters, ClusterStateResponse>(
				clusterStateSelector,
				(p, d) => this.RawDispatch.ClusterStateDispatch<ClusterStateResponse>(p)
			);
		}

		/// <inheritdoc />
		public IClusterStateResponse ClusterState(IClusterStateRequest clusterStateRequest)
		{
			return this.Dispatch<IClusterStateRequest, ClusterStateRequestParameters, ClusterStateResponse>(
				clusterStateRequest,
				(p, d) => this.RawDispatch.ClusterStateDispatch<ClusterStateResponse>(p)
			);
		}

		/// <inheritdoc />
		public Task<IClusterStateResponse> ClusterStateAsync(Func<ClusterStateDescriptor, ClusterStateDescriptor> clusterStateSelector = null)
		{
			clusterStateSelector = clusterStateSelector ?? (s => s);
			return this.DispatchAsync<ClusterStateDescriptor, ClusterStateRequestParameters, ClusterStateResponse, IClusterStateResponse>(
				clusterStateSelector,
				(p, d) => this.RawDispatch.ClusterStateDispatchAsync<ClusterStateResponse>(p)
			);
		}

		/// <inheritdoc />
		public Task<IClusterStateResponse> ClusterStateAsync(IClusterStateRequest clusterStateRequest)
		{
			return this.DispatchAsync<IClusterStateRequest, ClusterStateRequestParameters, ClusterStateResponse, IClusterStateResponse>(
				clusterStateRequest,
				(p, d) => this.RawDispatch.ClusterStateDispatchAsync<ClusterStateResponse>(p)
			);
		}


	}
}