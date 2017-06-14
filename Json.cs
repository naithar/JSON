//
// Json.cs
//
// Created by Sergey Minakov on 22.03.2016.
//
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;

//TODO: summary, comments.
using System.Text;

namespace mapp.core.json {

	public static class JsonExtensions {

		public static JsonObject GetValueFromIndexPath(this IJsonIndexer json, params object[] indexPath) {
			JsonObject jsonObject = null;

			foreach (var index in indexPath) {
				jsonObject = jsonObject == null ? json?[index] : jsonObject[index];
				if (jsonObject?.Object == null) {
					break;
				}
			}

			return jsonObject ?? new JsonObject();
		}
	}

	#region Interfaces

	public interface IJsonValueParser {

		int Int {
			get;
		}

		double Real {
			get;
		}

		bool Bool {
			get;
		}

		string String {
			get;
		}

		Dictionary<string, object> Dictionary {
			get;
		}

		List<object> List {
			get;
		}
	}

	public interface IJsonValueChild {

		object RootObject {
			get;
		}
	}

	public interface IJsonTreeChild {

		JsonObject Root {
			get;
		}
	}

	public interface IJsonIndexer {

		JsonObject this[params object[] indexPath] {
			get;
		}

		JsonObject this[object index] {
			get;
		}

		List<string> Keys {
			get;
		}

		int Count {
			get;
		}

		bool isEmpty {
			get;
		}
	}

	public interface IJsonRoot {
		void AddToRoot(string key, object value);
	}

	#endregion

	public class JsonObject: IJsonIndexer, IJsonValueParser, IJsonTreeChild {

		public object Object {
			private set;
			get;
		}

		public Type ObjectType {
			get {
				if (this.Object is JsonValue) {
					return (this.Object as JsonValue).Value.GetType();
				}

				return this.Object.GetType();
			}
		}

		public JsonObject Root {
			get {
				if (this.Object is IJsonValueChild) {
					var root = (this.Object as IJsonValueChild)?.RootObject;

					return new JsonObject(root);

				}

				return new JsonObject();
			}
		}

		public JsonObject() : this(null) {
		}

		public JsonObject(object @object) {
			this.Object = @object;
		}

		#region IJsonIndexer

		public JsonObject this[params object[] indexPath] {
			get {
				if (this.Object is IJsonIndexer) {
					var indexedObject = (this.Object as IJsonIndexer)[indexPath];
					return indexedObject;
				}

				return new JsonObject();
			}
		}

		public JsonObject this[object index] {
			get {
				if (this.Object is IJsonIndexer) {
					return (this.Object as IJsonIndexer)[index];
				} else if (index.ToString() == "..") {
					return this.Root;
				}

				return new JsonObject();
			}
		}

		public List<string> Keys {
			get {
				if (this.Object is IJsonIndexer) {
					return (this.Object as IJsonIndexer).Keys;
				}

				return new List<string>();
			}
		}

		public int Count {
			get {
				if (this.Object is IJsonIndexer) {
					return (this.Object as IJsonIndexer).Count;
				}

				return 0;
			}
		}

		public bool isEmpty {
			get {
				if (this.Object is IJsonIndexer) {
					return (this.Object as IJsonIndexer).isEmpty;
				}

				return this.Object == null;
			}
		}

		#endregion

		#region IJsonValueParser

		public int Int {
			get {
				if (this.Object is IJsonValueParser) {
					return (this.Object as IJsonValueParser)?.Int ?? 0;
				}
				int value = 0;
				string stringObject = this.Object?.ToString();
				int.TryParse(stringObject, out value);
				return value;
			}
		}

		public double Real {
			get {
				if (this.Object is IJsonValueParser) {
					return (this.Object as IJsonValueParser)?.Real ?? 0;
				}
				double value = 0;
				string stringObject = this.Object?.ToString();
				double.TryParse(stringObject, out value);
				return value;
			}
		}

		public bool Bool {
			get {
				if (this.Object is IJsonValueParser) {
					return (this.Object as IJsonValueParser)?.Bool ?? false;
				}
				bool value = false;
				string stringObject = this.Object?.ToString();
				if (!bool.TryParse(stringObject, out value)) {
					var intValue = 0;
					int.TryParse(stringObject, out intValue);
					value = intValue != 0;
				}
				return value;
			}
		}

		public string String {
			get {
				if (this.Object is IJsonValueParser) {
					return (this.Object as IJsonValueParser)?.String ?? "";
				}
				string stringObject = this.Object?.ToString() ?? "";
				return stringObject;
			}
		}

		public Dictionary<string, object> Dictionary {
			get {
				if (this.Object is IJsonValueParser) {
					return (this.Object as IJsonValueParser)?.Dictionary ?? new Dictionary<string, object>();
				}
				var dictionary = new Dictionary<string, object>();

				if (this.Object is JsonHashRoot) {
					var hash = this.Object as JsonHashRoot;
					dictionary.AddRange(hash);
				}

				return dictionary;
			}
		}

		public List<object> List {
			get {
				if (this.Object is IJsonValueParser) {
					return (this.Object as IJsonValueParser)?.List ?? new List<object>();
				}
				var list = new List<object>();

				if (this.Object is JsonArrayRoot) {
					var array = this.Object as JsonArrayRoot;
					list.AddRange(array);
				}

				return list;
			}
		}

		#endregion
	}

	public class JsonValue: Object, IJsonValueChild, IJsonValueParser {

		public object Value;

		#region IJsonValueChild

		public object RootObject {
			private set;
			get;
		}

		#endregion


		public JsonValue(object root) : base() {
			this.RootObject = root;
		}

		#region IJsonValueParser

		public int Int {
			get {
				int value = 0;
				string stringObject = this.Value?.ToString();
				int.TryParse(stringObject, out value);
				return value;
			}
		}

		public double Real {
			get {
				double value = 0;
				string stringObject = this.Value?.ToString();
				double.TryParse(stringObject, out value);
				return value;
			}
		}

		public bool Bool {
			get {
				bool value = false;
				string stringObject = this.Value?.ToString();
				if (!bool.TryParse(stringObject, out value)) {
					var intValue = 0;
					int.TryParse(stringObject, out intValue);
					value = intValue != 0;
				}
				return value;
			}
		}

		public string String {
			get {
				string stringObject = this.Value?.ToString() ?? "";
				return stringObject;
			}
		}

		public Dictionary<string, object> Dictionary {
			get {
				return new Dictionary<string, object>();
			}
		}

		public List<object> List {
			get {
				return new List<object>();
			}
		}

		#endregion

	}

	public class JsonHashRoot: Dictionary<string, object>, IJsonValueChild, IJsonRoot, IJsonIndexer, IJsonValueParser {

		#region IJsonValueChild

		public object RootObject {
			private set;
			get;
		}

		#endregion

		public JsonHashRoot(object root) : base() {
			this.RootObject = root;
		}

		#region IJsonRoot

		public void AddToRoot(string key, object value) {
			if (key == null
			    || value == null) {
				return;
			}

			this.Add(key, value);
		}

		#endregion

		#region IJsonIndexer

		public JsonObject this[params object[] indexPath] {
			get {
				var indexedObject = this.GetValueFromIndexPath(indexPath);
				return indexedObject;
			}
		}

		public JsonObject this[object index] {
			get {
				var dictionaryIndex = index.ToString().ToLower();

				if (this.ContainsKey(dictionaryIndex)) {
					var indexedObject = base[dictionaryIndex];
					var jsonObject = new JsonObject(indexedObject);
					return jsonObject;
				} else if (dictionaryIndex == "..") {
					return new JsonObject(this.RootObject);
				}

				return new JsonObject();
			}
		}

		public new List<string> Keys {
			get {
				return base.Keys.ToList();
			}
		}

		public new int Count {
			get {
				return base.Count;
			}
		}

		public bool isEmpty {
			get {
				return base.Count == 0;
			}
		}

		#endregion

		#region IJsonValueParser

		public int Int {
			get {
				return 0;
			}
		}

		public double Real {
			get {
				return 0;
			}
		}

		public bool Bool {
			get {
				return false;
			}
		}

		public string String {
			get {
				return "JsonHash";
			}
		}

		public Dictionary<string, object> Dictionary {
			get {
				return this;
			}
		}

		public List<object> List {
			get {
				return new List<object>();
			}
		}

		#endregion
	}

	public class JsonArrayRoot: List<object>, IJsonRoot, IJsonIndexer, IJsonValueChild {

		#region IJsonValueChild

		public object RootObject {
			private set;
			get;
		}

		#endregion

		public JsonArrayRoot(object root) : base() {
			this.RootObject = root;
		}

		#region IJsonRoot

		public void AddToRoot(string key, object value) {
			if (value == null) {
				return;
			}

			this.Add(value);
		}

		#endregion

		#region IJsonIndexer

		public JsonObject this[params object[] indexPath] {
			get {
				var indexedObject = this.GetValueFromIndexPath(indexPath);
				return indexedObject;
			}
		}

		public JsonObject this[object index] {
			get {
				int listIndex = 0;

				if (int.TryParse(index.ToString(), out listIndex)
				    && listIndex < this.Count) {
					var indexedObject = base[listIndex];
					var jsonObject = new JsonObject(indexedObject);
					return jsonObject;
				} else if (index.ToString() == "..") {
					return new JsonObject(this.RootObject);
				}

				return new JsonObject();
			}
		}

		public List<string> Keys {
			get {
				return new List<string>();
			}
		}

		public new int Count {
			get {
				return base.Count;
			}
		}

		public bool isEmpty {
			get {
				return base.Count == 0;
			}
		}

		#endregion

		#region IJsonValueParser

		public int Int {
			get {
				return 0;
			}
		}

		public double Real {
			get {
				return 0;
			}
		}

		public bool Bool {
			get {
				return false;
			}
		}

		public string String {
			get {
				return "JsonArray";
			}
		}

		public Dictionary<string, object> Dictionary {
			get {
				return new Dictionary<string, object>();
			}
		}

		public List<object> List {
			get {
				return this;
			}
		}

		#endregion
	}

	public class Json: IDisposable, IJsonIndexer, IJsonTreeChild {

		public static Json Empty {
			get {
				return new Json();
			}
		}

		public Json() {
			this.root = new JsonHashRoot(null);
		}

		public Json(Stream stream, bool disposeStream = true) {
			this.disposeStream = disposeStream;
			this.streamBase = stream;
			this.Perform();
		}

		public Json(String stringJson) {
			this.disposeStream = true;
			this.stringBase = stringJson;
			this.Perform();
		}

		#region IDisposable implementation

		private bool disposeStream;
		private bool disposed = false;

		public virtual void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		~Json() {
			this.Dispose(false);
		}

		protected virtual void Dispose(bool disposing) {
			if (!this.disposed) {
				if (disposing) {
					if (this.streamBase != null
					    && this.disposeStream) {
						this.streamBase.Dispose();
					}
				}
				this.disposed = true;
			}
		}

		#endregion

		#region IJsonIndexer

		public JsonObject this[params object[] indexPath] {
			get {
				var indexedObject = this.root?[indexPath];
				return indexedObject;
			}
		}

		public JsonObject this[object index] {
			get {
				var indexedObject = this.root?[index];
				return indexedObject ?? new JsonObject();
			}
		}

		public List<string> Keys {
			get {
				if (this.root == null) {
					return new List<string>();
				}

				return this.root.Keys;
			}
		}

		public int Count {
			get {
				if (this.root == null) {
					return 0;
				}

				return this.root.Count;
			}
		}

		public bool isEmpty {
			get {
				if (this.root == null) {
					return true;
				}
				return this.root.isEmpty;
			}
		}

		#endregion

		#region IJsonTreeChild

		public JsonObject Root {
			get {
				if (this.root == null) {
					return new JsonObject();
				}

				return new JsonObject(this.root);
			}
		}

		#endregion

		private JsonHashRoot root;

		private Stream streamBase;
		private String stringBase;

		public static Task<Json> Parse(Stream stream, bool disposeStream = true) {
			return Task.Run(() => {
				return new Json(stream, disposeStream);
			});
		}

		public static Task<Json> Parse(String stringJson) {
			return Task.Run(() => {
				return new Json(stringJson);
			});
		}

		public void Perform() {
			try {
				if (this.streamBase != null
				    && this.streamBase.CanSeek) {
					this.streamBase.Seek(0, SeekOrigin.Begin);
				}

				if (this.streamBase != null) {
					using (var streamReader = new StreamReader(this.streamBase,
						Encoding.UTF8,
						true,
						GlobalVariables.ByteReadLength,
						true)) {
						this.Perform(streamReader);
					}
					if (this.streamBase != null
					    && this.streamBase.CanSeek) {
						this.streamBase.Seek(0, SeekOrigin.Begin);
					}

				} else if (this.stringBase != null) {
					using (var stringReader = new StringReader(this.stringBase)) {
						this.Perform(stringReader);
					}
				}
			} catch (Exception ex) {
				Debug.WriteLine(ex);
			}
		}

		#region Perivate methods

		private void Perform(TextReader textReader) {
			this.root = new JsonHashRoot(null);

			using (var jsonReader = new JsonTextReader(textReader)) {
				this.PerformParse(this.root, jsonReader);
			}
		}

		private void ParseRootValue(IJsonRoot parentRoot,
		                            JsonTextReader reader,
		                            IJsonRoot innerRoot,
		                            string workingKey) {
			this.PerformParse(innerRoot, reader);
			parentRoot.AddToRoot(workingKey, innerRoot);
		}

		private void ParseBaseTypeValue(IJsonRoot parentRoot,
		                                JsonTextReader reader,
		                                string workingKey) {
			var value = new JsonValue(parentRoot);
			value.Value = reader.Value;
			parentRoot.AddToRoot(workingKey, value);
		}

		private void PerformParse(IJsonRoot root,
		                          JsonTextReader jReader) {
			string workingKey = null;

			while (jReader.Read()) {
				switch (jReader.TokenType) {
				case JsonToken.PropertyName:
					workingKey = jReader.Value?.ToString().ToLower();
					break;
				case JsonToken.StartObject:
					if (!object.ReferenceEquals(root, this.root)
					    || workingKey != null) {
						var newDictionaryRoot = new JsonHashRoot(root);
						this.ParseRootValue(root, jReader, newDictionaryRoot, workingKey);
					}
					break;
				case JsonToken.StartArray:
					{
						var newArrayRoot = new JsonArrayRoot(root);
						this.ParseRootValue(root, jReader, newArrayRoot, workingKey);
					}
					break;
				case JsonToken.Integer:
				case JsonToken.String:
				case JsonToken.Boolean:
				case JsonToken.Float:
				case JsonToken.Date:
				case JsonToken.Bytes:
					this.ParseBaseTypeValue(root, jReader, workingKey);
					break;
				case JsonToken.EndArray:
					return;
				case JsonToken.EndObject:
					return;
				default:
					break;
				}
			}
		}

		#endregion
	}
}
