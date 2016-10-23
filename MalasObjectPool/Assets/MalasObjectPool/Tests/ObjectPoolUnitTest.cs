using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
public partial class ObjectPool : MonoBehaviour
{
    public bool RunTests = true;

    void Start()
    {
        if (RunTests)
        {
            Debug.Log("*** Running Object Pool Unit Tests...");

            StartCoroutine(DefaultConstructorObject());
            StartCoroutine(DefaultConstructorObjectMany());
            StartCoroutine(ThresholdDefaultSuccess1());
            StartCoroutine(ThresholdDefaultSuccess2());
            StartCoroutine(ThresholdIncreased1());
            StartCoroutine(ThresholdIncreased2());
            StartCoroutine(Release1());
            StartCoroutine(Release2());
            StartCoroutine(GameObjectTest1());
            StartCoroutine(GameObjectTest2());
            StartCoroutine(AsyncImmediateInstantiation());
            StartCoroutine(AddTypeToPoolTest());

            PerformanceTest();
        }
    }

    IEnumerator DefaultConstructorObject()
    {
        //Given
        TestDataClass data = ObjectPool.Instance.Acquire<TestDataClass>();
        yield return new WaitForEndOfFrame();
        TestDataClass expected = new TestDataClass(1);

        //When
        bool equal = expected.CheckEqual(data);

        //Then
        Assert.IsTrue(equal);
    }

    IEnumerator DefaultConstructorObjectMany()
    {
        //Given
        List<TestDataClass> list = new List<TestDataClass>();
        TestDataClass expected = new TestDataClass(1);
        int count = 20;
        for (int i = 0; i < count; i++)
        {
            list.Add(ObjectPool.Instance.Acquire<TestDataClass>());
            yield return new WaitForEndOfFrame();
        }

        //When
        bool defaultConstructorObjectMany = true;
        foreach (TestDataClass d in list)
        {
            if (!expected.CheckEqual(d))
                defaultConstructorObjectMany = false;
        }

        //Then
        Assert.IsTrue(defaultConstructorObjectMany);
    }

    IEnumerator ThresholdDefaultSuccess1()
    {
        //Given
        for (int i = 0; i < 31; i++)
        {
            ObjectPool.Instance.Acquire<ThresholdDerivativeDefaultSuccess1>();
            yield return new WaitForEndOfFrame();
        }

        //When
        int count = ObjectPool.Instance.GetInstanceCountTotal<ThresholdDerivativeDefaultSuccess1>();

        //Then
        int expected = 32; //Count should be N * 2 of acquired items
        Assert.AreEqual(count, expected);
    }

    IEnumerator ThresholdDefaultSuccess2()
    {
        //Given
        for (int i = 0; i < 32; i++)
        {
            ObjectPool.Instance.Acquire<ThresholdDerivativeDefaultSuccess2>();
            yield return new WaitForEndOfFrame();
        }

        //When
        int count = ObjectPool.Instance.GetInstanceCountTotal<ThresholdDerivativeDefaultSuccess2>();

        //Then
        int expected = 64; //Count should be N * 2 of acquired items
        Assert.AreEqual(count, expected);
    }

    IEnumerator ThresholdIncreased1()
    {
        //Given
        ObjectPool.Instance.Acquire<ThresholdDerivativeIncreased1>();
        yield return new WaitForEndOfFrame();
        ObjectPool.Instance.SetLowerInstantiationThreshold<ThresholdDerivativeIncreased1>(2);

        for (int i = 0; i < 14; i++)
        {
            ObjectPool.Instance.Acquire<ThresholdDerivativeIncreased1>();
            yield return new WaitForEndOfFrame();
        }

        //When
        int count = ObjectPool.Instance.GetInstanceCountTotal<ThresholdDerivativeIncreased1>();

        //Then
        int expected = 16;
        Assert.AreEqual(count, expected);
    }

    IEnumerator ThresholdIncreased2()
    {
        //Given
        ObjectPool.Instance.Acquire<ThresholdDerivativeIncreased2>();
        yield return new WaitForEndOfFrame();
        ObjectPool.Instance.SetLowerInstantiationThreshold<ThresholdDerivativeIncreased2>(2);

        for (int i = 0; i < 15; i++)
        {
            ObjectPool.Instance.Acquire<ThresholdDerivativeIncreased2>();
            yield return new WaitForEndOfFrame();
        }

        //When
        int count = ObjectPool.Instance.GetInstanceCountTotal<ThresholdDerivativeIncreased2>();

        //Then
        int expected = 32;
        Assert.AreEqual(count, expected);
    }

    IEnumerator Release1()
    {
        //Given
        ReleaseDerivative1 obj = ObjectPool.Instance.Acquire<ReleaseDerivative1>();
        ObjectPool.Instance.SetLowerInstantiationThreshold<ReleaseDerivative1>(2);

        for (int i = 0; i < 13; i++)
        {
            ObjectPool.Instance.Acquire<ReleaseDerivative1>();
            yield return new WaitForEndOfFrame();
        }

        //When
        ObjectPool.Instance.Release<ReleaseDerivative1>(obj);
        ObjectPool.Instance.Acquire<ReleaseDerivative1>();
        yield return new WaitForEndOfFrame();

        int count = ObjectPool.Instance.GetInstanceCountTotal<ReleaseDerivative1>();

        //Then
        int expected = 16;
        Assert.AreEqual(count, expected);
    }

    IEnumerator Release2()
    {
        //Given
        ReleaseDerivative2 obj = ObjectPool.Instance.Acquire<ReleaseDerivative2>();
        yield return new WaitForEndOfFrame();
        ObjectPool.Instance.SetLowerInstantiationThreshold<ReleaseDerivative2>(2);

        for (int i = 0; i < 14; i++)
        {
            ObjectPool.Instance.Acquire<ReleaseDerivative2>();
            yield return new WaitForEndOfFrame();
        }

        //When
        ObjectPool.Instance.Release<ReleaseDerivative2>(obj);
        ObjectPool.Instance.Acquire<ReleaseDerivative2>();
        yield return new WaitForEndOfFrame();

        int count = ObjectPool.Instance.GetInstanceCountTotal<ReleaseDerivative2>();

        //Then
        int expected = 16;
        Assert.AreNotEqual(count, expected);
    }

    IEnumerator GameObjectTest1()
    {
        //Given
        ObjectPoolGameObjectTestScript obj = ObjectPool.Instance.Acquire<ObjectPoolGameObjectTestScript>();
        yield return new WaitForEndOfFrame();

        //Then
        Assert.IsNotNull(obj);
        Assert.IsNotNull(obj.gameObject);
    }

    IEnumerator GameObjectTest2()
    {
        for (int i = 0; i < 20; i++)
        {
            //Given
            ObjectPoolGameObjectTestScript obj = ObjectPool.Instance.Acquire<ObjectPoolGameObjectTestScript>();
            yield return new WaitForEndOfFrame();

            //Then
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.gameObject);
        }
    }

    IEnumerator AsyncImmediateInstantiation()
    {
        //Given
        yield return new WaitForEndOfFrame();        

        for (int i = 0; i < 20; i++)
        {
            AsyncImmediateDerivative obj = ObjectPool.Instance.Acquire<AsyncImmediateDerivative>();

            //Then
            TestDataClass expected = new TestDataClass(1);
            Assert.IsTrue(obj.CheckEqual(expected));
        }
    }

    IEnumerator AddTypeToPoolTest()
    {
        //Given
        yield return new WaitForEndOfFrame();

        AddTypeToPoolDerivative d = new AddTypeToPoolDerivative();
        ObjectPool.Instance.AddTypeToPool(d);

        AddTypeToPoolDerivative d2 = ObjectPool.Instance.Acquire<AddTypeToPoolDerivative>();

        //When
        int count = ObjectPool.Instance.GetInstanceCountTotal<AddTypeToPoolDerivative>();

        //Then
        int expected = 2;
        Assert.AreEqual(count, expected);
    }

    void PerformanceTest()
    {
        ObjectPoolGameObjectTestScript reference = ObjectPool.Instance.Acquire<ObjectPoolGameObjectTestScript>();

        Stopwatch without = new Stopwatch();
        Stopwatch generic = new Stopwatch();

        int num = 10000;

        //Without the use of an object pool
        without.Start();
        for (int i = 0; i < num; ++i)
        {
            GameObject go = GameObject.Instantiate<GameObject>(reference.gameObject);

            if (i % 2 == 0)
            {
                GameObject.Destroy(go);
            }
        }
        without.Stop();

        //Generic object pool
        generic.Start();
        for (int i = 0; i < num; ++i)
        {
            ObjectPoolGameObjectTestScript t = ObjectPool.Instance.Acquire<ObjectPoolGameObjectTestScript>();

            if (i % 2 == 0)
            {
                ObjectPool.Instance.Release<ObjectPoolGameObjectTestScript>(t);
            }
        }
        generic.Stop();

        Debug.Log("Performance, " + num + " entities - Without: " + without.Elapsed);
        Debug.Log("Performance, " + num + " entities - Generic: " + generic.Elapsed);
    }

    class TestDataClass
    {
        public int myInt;
        public string myString;
        public List<string> list = new List<string>();
        public Vector3 myVector;

        public TestDataClass() : this(1)
        { }

        public TestDataClass(int num)
        {
            myInt = num;
            myString = "Num: " + num.ToString();
            for (int i = 0; i < myInt; i++)
            {
                list.Add(" entry: " + i);
            }
            myVector = new Vector3(myInt, -myInt, myInt);
        }

        public bool CheckEqual(TestDataClass data)
        {
            bool res = true;

            if (data.myInt != this.myInt) res = false;
            if (data.myString != this.myString) res = false;
            if (data.myVector != this.myVector) res = false;

            for (int i = 0; i < data.list.Count; i++) if (data.list[i] != this.list[i]) res = false;

            return res;
        }
    }
    class ThresholdDerivativeDefaultSuccess1 : TestDataClass { }
    class ThresholdDerivativeDefaultSuccess2 : TestDataClass { }
    class ThresholdDerivativeIncreased1 : TestDataClass { }
    class ThresholdDerivativeIncreased2 : TestDataClass { }
    class ReleaseDerivative1 : TestDataClass { }
    class ReleaseDerivative2 : TestDataClass { }
    class AsyncImmediateDerivative : TestDataClass { }
    class PerformanceDerivative1 : TestDataClass { }
    class PerformanceDerivative2 : TestDataClass { }
    class PerformanceDerivative3 : TestDataClass { }
    class AddTypeToPoolDerivative : TestDataClass { }
}

#endif
