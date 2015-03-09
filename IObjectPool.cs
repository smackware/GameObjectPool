using UnityEngine;
using System.Collections;

public interface IObjectPool<T> {

	T Borrow();

	void Revert(T obj);
}
