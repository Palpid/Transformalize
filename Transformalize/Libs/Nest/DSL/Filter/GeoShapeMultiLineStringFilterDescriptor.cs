﻿using System.Collections.Generic;
using Transformalize.Libs.Newtonsoft.Json;
using Transformalize.Libs.Nest.Domain.Geometry;
using Transformalize.Libs.Nest.Domain.Marker;
using Transformalize.Libs.Nest.Extensions;

namespace Transformalize.Libs.Nest.DSL.Filter
{
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	public interface IGeoShapeMultiLineStringFilter : IGeoShapeBaseFilter
	{
		[JsonProperty("shape")]
		IMultiLineStringGeoShape Shape { get; set; }
	}

	public class GeoShapeMultiLineStringFilter : PlainFilter, IGeoShapeMultiLineStringFilter
	{
		protected internal override void WrapInContainer(IFilterContainer container)
		{
			container.GeoShape = this;
		}

		public PropertyPathMarker Field { get; set; }

		public IMultiLineStringGeoShape Shape { get; set; }
	}

	public class GeoShapeMultiLineStringFilterDescriptor : FilterBase, IGeoShapeMultiLineStringFilter
	{
		IGeoShapeMultiLineStringFilter Self { get { return this; } }

		bool IFilter.IsConditionless
		{
			get
			{
				return this.Self.Shape == null || !this.Self.Shape.Coordinates.HasAny();
			}
		}

		PropertyPathMarker IFieldNameFilter.Field { get; set; }
		IMultiLineStringGeoShape IGeoShapeMultiLineStringFilter.Shape { get; set; }

		public GeoShapeMultiLineStringFilterDescriptor Coordinates(IEnumerable<IEnumerable<IEnumerable<double>>> coordinates)
		{
			if (this.Self.Shape == null)
				this.Self.Shape = new MultiLineStringGeoShape();
			this.Self.Shape.Coordinates = coordinates;
			return this;
		}
	}

}