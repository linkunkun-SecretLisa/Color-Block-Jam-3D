using UnityEngine;

public class SizeTriggerDetector : MonoBehaviour
{
    [SerializeField] private float tolerance = 0.01f;
    [SerializeField] private string targetTag = "Item";

    // Bu objenin Collider'ı
    private Collider myCollider;
    
    void Start()
    {
        myCollider = GetComponent<Collider>();
        if(myCollider == null)
        {
            Debug.LogError("SizeTriggerDetector: Bu objede Collider bulunamadı!");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // İsteğe bağlı: sadece belirli tag'e sahip objeleri kontrol etmek için
        if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag))
            return;
        
        Collider otherCollider = other;
        // Her iki collider'ın boyutlarını alıyoruz
        Vector3 mySize = myCollider.bounds.size;
        Vector3 otherSize = otherCollider.bounds.size;
        
        if(AreSizesEqual(mySize, otherSize))
        {
            Debug.Log($"Boyutlar eşleşti: {mySize} == {otherSize}");
            // Eşleşme durumunda yapılacak işlemleri buraya ekleyebilirsiniz.
        }
        else
        {
            Debug.Log($"Boyutlar eşleşmiyor: {mySize} != {otherSize}");
        }
    }
    
    // Vector3 boyutlarını tolerans dahilinde karşılaştırır.
    private bool AreSizesEqual(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.x - b.x) < tolerance &&
               Mathf.Abs(a.y - b.y) < tolerance &&
               Mathf.Abs(a.z - b.z) < tolerance;
    }
}