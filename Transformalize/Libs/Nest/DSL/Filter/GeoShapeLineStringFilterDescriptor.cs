﻿using System.Collections.Generic;
using Transformalize.Libs.Newtonsoft.Json;
using Transformalize.Libs.Nest.Domain.Geometry;
using Transformalize.Libs.Nest.Domain.Marker;
using Transformalize.Libs.Nest.Extensions;

namespace Transformalize.Libs.Nest.DSL.Filter
{
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	public interface IGeoShapeLineStringFilter : IGeoShapeBaseFilter
	{
		[JsonProperty("shape")]
		ILineStringGeoShape Shape { get; set; }
	}

	public class GeoShapeLineStringFilter : PlainFilter, IGeoShapeLineStringFilter
	{
		protected internal override void WrapInContainer(IFilterContainer container)
		{
			container.GeoShape = this;
		}

		public PropertyPathMarker Field { get; set; }

		public ILineStringGeoShape Shape { get; set; }
	}

	public class GeoShapeLineStringFilterDescriptor : FilterBase, IGeoShapeLineStringFilter
	{
		IGeoShapeLineStringFilter Self { get { return this; } }

		bool IFilter.IsConditionless
		{
			get
			{
				return this.Self.Shape == null || !this.Self.Shape.Coordinates.HasAny();
			}
		}

		PropertyPathMarker IFieldNameFilter.Field { get; set; }
		ILineStringGeoShape IGeoShapeLineStringFilter.Shape { get; set; }

		public GeoShapeLineStringFilterDescriptor Coordinates(IEnumerable<IEnumerable<double>> coordinates)
		{
			if (this.Self.Shape == null)
				this.Self.Shape = new LineStringGeoShape();
			this.Self.Shape.Coordinates = coordinates;
			return this;
		}
	}

}