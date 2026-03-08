using Cavrnus.Base.Math;
using Cavrnus.Comm.Prop.BoolProp;
using Cavrnus.Comm.Prop.ColorProp;
using Cavrnus.Comm.Prop.ScalarProp;
using Cavrnus.Comm.Prop.StringProp;
using Cavrnus.Comm.Prop.TransformProp;
using Cavrnus.Comm.Prop.VectorProp;
using System;
using System.Collections.Generic;
using Cavrnus.Comm.Comm.LocalTypes;
using Cavrnus.Comm.Prop.Gen;

namespace Cavrnus.SpatialConnector.Core.HistoryBuilder
{
    internal static class TrackedItemStringConverter
    {
        public static List<Tuple<double, string, string>> TrackedPropertyToReadableChanges(ITrackedProperty prop)
        {
            List<Tuple<double, string, string>> res = new List<Tuple<double, string, string>>();

            if (prop is TrackedProperty<double, ScalarPropertyMetadata> scalarProp)
            {
                foreach (var change in scalarProp.generatorChanges)
                {
                    string changeGenStr = "";
                    if (change.gen is ScalarGeneratorConst constGen)
                    {
                        changeGenStr = constGen.Value.ToString();
                    }
                    if (change.gen is ScalarGeneratorApproach appGen)
                    {
                        if (appGen.To is ScalarGeneratorConst cg)
                            changeGenStr = cg.Value.ToString();
                    }

                    res.Add(new Tuple<double, string, string>(change.historyTime, change.connId, changeGenStr));
                }
            }

            if (prop is TrackedProperty<Float4, VectorPropertyMetadata> vectorProp)
            {
                foreach (var change in vectorProp.generatorChanges)
                {
                    string changeGenStr = VectorGenToString(change.gen);

                    res.Add(new Tuple<double, string, string>(change.historyTime, change.connId, changeGenStr));
                }
            }

            if (prop is TrackedProperty<TransformComplete, TransformPropertyMetadata> transformProp)
            {
                foreach (var change in transformProp.generatorChanges)
                {
                    string changeGenStr = TransformGenToString(change.gen);

                    res.Add(new Tuple<double, string, string>(change.historyTime, change.connId, changeGenStr));
                }
            }

            if (prop is TrackedProperty<string, StringPropertyMetadata> stringProp)
            {
                foreach (var change in stringProp.generatorChanges)
                {
                    string changeGenStr = "";
                    if (change.gen is StringGeneratorConst constGen)
                    {
                        changeGenStr = constGen.Value.ToString();
                    }

                    res.Add(new Tuple<double, string, string>(change.historyTime, change.connId, changeGenStr));
                }
            }

            if (prop is TrackedProperty<Color4F, ColorPropertyMetadata> colorProp)
            {
                foreach (var change in colorProp.generatorChanges)
                {
                    string changeGenStr = "";
                    if (change.gen is ColorGeneratorConst constGen)
                    {
                        changeGenStr = constGen.Value.ToString();
                    }

                    res.Add(new Tuple<double, string, string>(change.historyTime, change.connId, changeGenStr));
                }
            }

            if (prop is TrackedProperty<bool, BooleanPropertyMetadata> boolProp)
            {
                foreach (var change in boolProp.generatorChanges)
                {
                    string changeGenStr = "";
                    if (change.gen is BooleanGeneratorConst constGen)
                    {
                        changeGenStr = constGen.Value.ToString();
                    }

                    res.Add(new Tuple<double, string, string>(change.historyTime, change.connId, changeGenStr));
                }
            }

            return res;
        }

        private static string VectorGenToString(IGenerator<Float4> gen)
        {
            if (gen == null)
                return "";

            if (gen is VectorGeneratorConst constGen)
            {
                return constGen.Value.ToString();
            }
            if (gen is VectorGeneratorApproach appGen)
            {
                if (appGen.To is VectorGeneratorConst cg)
                    return cg.Value.ToString();
            }

            return "";
        }

        private static string TransformGenToString(IGenerator<TransformComplete> gen)
        {
            if (gen == null)
                return "";

            if (gen is TransformSetGeneratorSrt srt)
            {
                return TransformSrtToString(srt);
            }
            if (gen is TransformSetGeneratorApproach appGen)
            {
                if (appGen.To is TransformSetGeneratorSrt srtGen)
                    return TransformSrtToString(srtGen);
            }

            return "";
        }

        private static string TransformSrtToString(TransformSetGeneratorSrt srt)
        {
            return $"{VectorGenToString(srt.TransformPos)}, {VectorGenToString(srt.RotationEuler)}, {VectorGenToString(srt.Scale)}";
        }

        public static List<Tuple<double, string, string>> TrackedObjectToReadableChanges(TrackedObject ob)
        {
			List<Tuple<double, string, string>> res = new List<Tuple<double, string, string>>();

            foreach(var creationEvent in ob.createdWhens)
            {
                string obStr = "";


                if (ob.creation.ObjectType is ContentTypeWellKnownId id)
                    obStr = id.WellKnownId;
				if (ob.creation.ObjectType is ContentTypeChatEntry chat)
					obStr = $"Chat \"{chat.ChatText}\"";

				obStr += " - ";
                obStr += creationEvent.Item3 ? "Created" : "Destroyed";

				res.Add(new Tuple<double, string, string>(creationEvent.Item1, creationEvent.Item2, obStr));
            }

            return res;
		}

	}
}
