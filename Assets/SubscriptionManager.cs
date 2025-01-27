using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class SubscriptionManager : MonoBehaviour, IDetailedStoreListener
{
    public static SubscriptionManager Instance;
    // IAP = Achat In App
    private static IStoreController storeController;          // Gère les transactions
    private static IExtensionProvider storeExtensionProvider; // Fournit des extensions
    private const string subscriptionProductId = "your_subscription_id"; // L'ID défini dans la console (Google/Apple)

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (storeController == null)
        {
            InitializePurchasing();
        }

        AuthenticationService.Instance.SignedIn += () =>
        {
            if (storeController == null)
            {
                InitializePurchasing();
            }
        };
    }

    public void InitializePurchasing()
    {
        if (IsInitialized()) return;

        // Configure les produits disponibles
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Ajoute l'abonnement
        builder.AddProduct(subscriptionProductId, ProductType.Subscription);

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }


    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
        Debug.Log("IAP initialisé !");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"IAP a échoué à s'initialiser : {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"IAP a échoué à s'initialiser : {error}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"Achat échoué : {failureReason}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        throw new System.NotImplementedException();
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (args.purchasedProduct.definition.id == subscriptionProductId)
        {
            Debug.Log("Abonnement activé !");
            // Activer les fonctionnalités liées à l'abonnement
            AuthManager.Instance.MyAccount.SetSubscription(true);
        }
        else
        {
            Debug.LogError("Produit non reconnu !");
        }

        return PurchaseProcessingResult.Complete;
    }


    public void BuySubscription()
    {
        if (IsInitialized())
        {
            storeController.InitiatePurchase(subscriptionProductId);
            IsUserSubscribed();
        }
        else
        {
            Debug.LogError("IAP n'est pas initialisé !");
        }
    }

    public void CancelSubscription()
    {
        // Doit être géré par Google ou Apple
        if (IsInitialized())
        {
            AuthManager.Instance.MyAccount.SetSubscription(false);
            IsUserSubscribed();
        }
        else
        {
            Debug.LogError("IAP n'est pas initialisé !");
        }
    }

    public bool IsUserSubscribed()
    {
        if (storeController != null)
        {
            Product subscription = storeController.products.WithID(subscriptionProductId);
            if (subscription != null && subscription.hasReceipt)
            {
                Debug.Log("L'utilisateur est abonné.");
                return true;
            }
        }

        Debug.Log("L'utilisateur n'est pas abonné.");
        return false;
    }

}