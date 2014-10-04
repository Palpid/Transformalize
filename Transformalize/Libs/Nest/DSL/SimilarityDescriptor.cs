﻿using System;
using Transformalize.Libs.Nest.Domain;
using Transformalize.Libs.Nest.Domain.Similarity;
using Transformalize.Libs.Nest.Extensions;

namespace Transformalize.Libs.Nest.DSL
{
	public class SimilarityDescriptor
	{
		internal readonly SimilaritySettings _SimilaritySettings;

		public SimilarityDescriptor()
		{
			this._SimilaritySettings = new SimilaritySettings();
		}

		public SimilarityDescriptor(SimilaritySettings settings)
		{
			this._SimilaritySettings = settings;
		}

		public SimilarityDescriptor CustomSimilarities(
			Func<FluentDictionary<string, SimilarityBase>, FluentDictionary<string, SimilarityBase>> similaritySelector)
		{
			similaritySelector.ThrowIfNull("similaritySelector");
			var similarities = new FluentDictionary<string, SimilarityBase>(this._SimilaritySettings.CustomSimilarities);
			var newSimilarities = similaritySelector(similarities);
			this._SimilaritySettings.CustomSimilarities = newSimilarities;
			return this;
		}

		public SimilarityDescriptor Default(string defaultSimilarity)
		{
			this._SimilaritySettings.Default = defaultSimilarity;
			return this;
		}
	}
}