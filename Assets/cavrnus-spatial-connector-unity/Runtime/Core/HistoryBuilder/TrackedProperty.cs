using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Cavrnus.Base.Core;
using Cavrnus.Base.Math;
using Cavrnus.Base.Settings;
using Cavrnus.Comm.Prop;
using Cavrnus.Comm.Prop.BoolProp;
using Cavrnus.Comm.Prop.ColorProp;
using Cavrnus.Comm.Prop.Gen;
using Cavrnus.Comm.Prop.JournalInterop;
using Cavrnus.Comm.Prop.JsonProp;
using Cavrnus.Comm.Prop.LinkProp;
using Cavrnus.Comm.Prop.ScalarProp;
using Cavrnus.Comm.Prop.StringProp;
using Cavrnus.Comm.Prop.TransformProp;
using Cavrnus.Comm.Prop.VectorProp;

namespace Cavrnus.SpatialConnector.Core.HistoryBuilder
{
	public interface ITrackedProperty
	{
		IProperty Property { get; }

		void Emit(string cat);
	}
	public class TrackedProperty<T, TMetadata> : ITrackedProperty where TMetadata : PropertyMetadata
	{
		public class TrackedGeneratorChange {
			public double historyTime;
			public string connId;
			public bool wasTransient;
			public IGenerator<T> gen;

			public TrackedGeneratorChange(double historyTime, string connId, bool wasTransient, IGenerator<T> gen)
			{
				this.historyTime = historyTime;
				this.connId = connId;
				this.wasTransient = wasTransient;
				this.gen = gen;
			}
        }

		private IActiveHistoryTimeProvider htp;

		private AProperty<T, TMetadata> property;
		public IProperty Property => property;

		public List<TrackedGeneratorChange> generatorChanges = new List<TrackedGeneratorChange>();
		public T initialDefault;

		public bool hasDefault;
		public T defaultValue;
		public TMetadata finalMeta;

		public TrackedProperty(AProperty<T, TMetadata> property, IActiveHistoryTimeProvider htp, Func<T, IGenerator<T>> constGen)
		{
			this.htp = htp;
			this.property = property;

			property.DefaultValue.Bind(v =>
			{
				defaultValue = v;
			});
			property.BaseMeta.Bind(m =>
			{
				finalMeta = (m as TMetadata) ?? finalMeta;
			});
			property.LiveGenerator.Bind((gen) =>
			{
				if (generatorChanges.Count > 0 && generatorChanges.Last().historyTime >= htp.ActiveHistoryTime - .001)
					generatorChanges.RemoveAt(generatorChanges.Count - 1);

				if (gen is IWrappedGenerator wg)
					gen = wg.InternalGenerator as IGenerator<T>;

				if (gen != null)
					generatorChanges.Add(new TrackedGeneratorChange(htp.ActiveHistoryTime, htp.ActiveUserId, htp.ActiveIsTransient, gen));
				else if (hasDefault)
					generatorChanges.Add(new TrackedGeneratorChange(htp.ActiveHistoryTime, htp.ActiveUserId, htp.ActiveIsTransient, constGen(property.DefaultValue == null ? default(T) : property.DefaultValue.Value)));
				else
					generatorChanges.Add(new TrackedGeneratorChange(htp.ActiveHistoryTime, htp.ActiveUserId, htp.ActiveIsTransient, null));//constGen(property.DefaultValue == null ? default(T) : property.DefaultValue.Value)));
			});
		}

		public void Emit(string cat)
		{
			DebugOutput.Out(cat, $"TrackedProp: {this.Property.AbsoluteId} with {generatorChanges.Count} value-gen changes: {String.Join(" > ", generatorChanges.Select(g => g.gen.ToString()))}");
		}
	}

	public static class TrackedPropertyFactory
	{
		public static ITrackedProperty Track(IProperty p, IActiveHistoryTimeProvider htp)
		{
			if (p is ScalarProperty sp)
				return new TrackedProperty<double, ScalarPropertyMetadata>(sp, htp, (v) => new ScalarGeneratorConst(v));
			if (p is StringProperty st)
				return new TrackedProperty<string, StringPropertyMetadata>(st, htp, (v) => new StringGeneratorConst(v));
			if (p is LinkProperty l)
				return new TrackedProperty<PropertyId, LinkPropertyMetadata>(l, htp, (v) => new LinkGeneratorConst(v));
			if (p is VectorProperty v)
				return new TrackedProperty<Float4, VectorPropertyMetadata>(v, htp, (v) => new VectorGeneratorConst(v));
			if (p is ColorProperty c)
				return new TrackedProperty<Color4F, ColorPropertyMetadata>(c, htp, (v) => new ColorGeneratorConst(v));
			if (p is BooleanProperty b)
				return new TrackedProperty<bool, BooleanPropertyMetadata>(b, htp, (v) => new BooleanGeneratorConst(v));
			if (p is TransformProperty tr)
				return new TrackedProperty<TransformComplete, TransformPropertyMetadata>(tr, htp, (v) => v.ToConstGenerator());
			if (p is JsonProperty j)
				return new TrackedProperty<JsonObject, JsonPropertyMetadata>(j, htp, (v) => new JsonGeneratorConst(v));
			return null;
		}

		public static List<TrackedProperty<T, TMetadata>.TrackedGeneratorChange> MergeAndSortPropertyChanges<T, TMetadata>
			(IEnumerable<TrackedProperty<T, TMetadata>> tps)
			where TMetadata : PropertyMetadata
		{
			SortedList<double, TrackedProperty<T, TMetadata>.TrackedGeneratorChange> sorted =
				new SortedList<double, TrackedProperty<T, TMetadata>.TrackedGeneratorChange>();

			foreach (var tp in tps)
			{
				foreach (var tgc in tp.generatorChanges)
				{
					try
					{
						sorted.Add(tgc.historyTime, tgc);
					}
					catch (ArgumentException) // same time. Ignore and continue.
					{
					}
				}
			}

			return sorted.Values.ToList();
		}
	}
}