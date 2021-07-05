public interface IPoolObject {
	string Key { get; set; }
	int StartingCount { get; set; }

	void Recycle();
	void OnActivate();
}

