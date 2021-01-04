using System;
using System.Collections.Generic;

namespace DeckConsole {
	[Serializable()]
	public class InvalidDeckException : System.Exception {
		public InvalidDeckException() : base() { }
		public InvalidDeckException(string message) : base(message) { }
		public InvalidDeckException(string message, System.Exception inner) : base(message, inner) { }

		// A constructor is needed for serialization when an 
		// exception propogates from a remoting server to the client
		protected InvalidDeckException(System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	public class DeckCollection<T> : List<T> {
		private Noise r_instance;
		public int index { get; private set; } = 0;

		private long prev = 0;

		#region Constructors
		public DeckCollection() : base() {
			Init();
		}
		public DeckCollection(int capacity) : base(capacity) {
			Init();
		}
		public DeckCollection(IEnumerable<T> collection) : base(collection) {
			Init();
		}

		public DeckCollection(int capacity, uint seed = 0) : base(capacity) {
			r_instance = new Noise(seed == 0 ? (uint)DateTime.Now.Ticks : seed);
			prev = r_instance.position;
		}

		public DeckCollection(Noise rInstance, int index) {
			this.index = index;
			r_instance = rInstance;
			prev = r_instance.position;
		}

		public void Init() {
			r_instance = new Noise((uint)DateTime.Now.Ticks);
			prev = r_instance.position;
		}

		//int capacity
		//IEnumerable<T> collection
		#endregion

		#region Public Methods

		public void Seed(uint seed) {
			r_instance = new Noise(seed);
			prev = r_instance.position;
		}

		public void Shuffle() {
			prev = r_instance.position;
			for (int i = 0; i < Count; i++) {
				Swap(i, r_instance.Next(Count));
			}
		}

		public delegate Type EqualCheck<Type>(T elem);

		public T Draw() {
			T ret = this[index++];
			if (index >= Count) {
				index = 0;
				Shuffle();
			}
			return ret;
		}

		public T Drawif<S>(S equalTo, EqualCheck<S> check) {
			int count = 0;
			T ret = this[index++];
			while (check(this[index]).Equals(equalTo)) {
				ret = this[index++];
				if (index >= Count) {
					index = 0;
					Shuffle();
					if (++count >= 2) {
						throw new InvalidDeckException("Tried to check against entire deck against condition, all elements of the deck satisfied the condition. \nEnsure at least one element of the deck is satistfactorily different from the others.");
					}
				}
			}
			return ret;
		}

		public void Add(T item, bool shuffle = true) {
			base.Add(item);
			if (shuffle) { Shuffle(); }
		}

		#region Removal Functions
		public bool Remove(T item, bool shuffle = true) {
			bool ret = base.Remove(item);
			if (shuffle) { Shuffle(); }
			return ret;
		}
		public int RemoveAll(Predicate<T> match, bool shuffle = true) {
			int ret = base.RemoveAll(match);
			if (shuffle) { Shuffle(); }
			return ret;
		}
		public void RemoveAt(int index, bool shuffle = true) {
			base.RemoveAt(index);
			if (shuffle) { Shuffle(); }
		}
		public void RemoveRange(int index, int count, bool shuffle = true) {
			base.RemoveRange(index, count);
			if (shuffle) { Shuffle(); }
		}
		#endregion

		#endregion

		#region Private Methods
		private void Swap(int index1, int index2) {
			T thing = this[index1];
			this[index1] = this[index2];
			this[index2] = thing;
		}

		public override bool Equals(object obj) {
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override string ToString() {
			return base.ToString();
		}
		#endregion

	}
}
