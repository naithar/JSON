using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace mapp.core.json {
	public class JsonConverter {
		public enum EventType {
			StartObject,
			EndObject,
			StartArray,
			EndArray,
			Value
		}

		public class ConvertionContextItem {
			internal ConvertionContextItem() {

			}

			public bool IsArray { get; internal set; }

			public string Name { get; internal set; }

			public object Instance { get; internal set; }
		}

		public class ConvertionContext {
			private List<ConvertionContextItem> _stack;

			public IList<ConvertionContextItem> Stack { get { return _stack; } }

			public ConvertionContextItem Top { get { return _stack.Count > 0 ? _stack[_stack.Count - 1] : null; } }

			internal void Push(ConvertionContextItem item) {
				_stack.Add(item);
			}

			internal ConvertionContextItem Pop() {
				var lastIdx = _stack.Count - 1;
				if (lastIdx < 0)
					return null;
				var item = _stack[lastIdx];
				_stack.RemoveAt(lastIdx);
				return item;
			}

			internal bool IsHandlingEnabled {
				get {
					return (Top?.Instance != null || _stack.Count == 0);
				}
			}

			public bool AbortParsing;

			internal ConvertionContext() {
				_stack = new List<ConvertionContextItem>();
				AbortParsing = false;
			}
		}

		private Func<ConvertionContext, EventType, string, string, object> _evHandler;


		public JsonConverter(Func<ConvertionContext, EventType, string, string, object> evHandler) {
			_evHandler = evHandler;
		}

		public void Perform(TextReader reader) {
			if (_evHandler == null)
				return;

			string propertyName = null;
			var context = new ConvertionContext();
			var jReader = new JsonTextReader(reader);
			while (!context.AbortParsing && jReader.Read()) {
				switch (jReader.TokenType) {
				case JsonToken.PropertyName:
					propertyName = jReader.Value?.ToString();
					break;
				case JsonToken.StartObject:
					context.Push(new ConvertionContextItem {
						Name = propertyName,
						IsArray = false,
						Instance = context.IsHandlingEnabled ? _evHandler(context, EventType.StartObject, propertyName, jReader.Value?.ToString()) : null
					});
					break;
				case JsonToken.EndObject:
					if (context.IsHandlingEnabled)
						_evHandler(context, EventType.EndObject, propertyName, jReader.Value?.ToString());
					context.Pop();
					break;
				case JsonToken.StartArray:
					context.Push(new ConvertionContextItem {
						Name = propertyName,
						IsArray = true,
						Instance = context.IsHandlingEnabled ? _evHandler(context, EventType.StartArray, propertyName, jReader.Value?.ToString()) : null
					});
					break;
				case JsonToken.EndArray:
					if (context.IsHandlingEnabled)
						_evHandler(context, EventType.EndArray, propertyName, jReader.Value?.ToString());
					context.Pop();
					break;
				case JsonToken.Integer:
				case JsonToken.String:
					if (context.IsHandlingEnabled)
						_evHandler(context, EventType.Value, propertyName, jReader.Value?.ToString());
					break;

				}
			}
		}

	}
}
