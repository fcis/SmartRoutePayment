export const environment = {
  production: false,
  
  // Your Backend API (for hash generation & logging)
  apiUrl: 'https://localhost:7001',
  
  // PayOne Configuration (from your appsettings.json)
  payone: {
    // PayOne Direct Post URL (Angular posts card data here directly)
    directPostUrl: 'https://smartroute-test.payone.io/SmartRoutePaymentWeb/SRPayMsgHandler',
    
    // UI Configuration (non-sensitive - safe for frontend)
    language: 'en',              // en or ar
    currencyIsoCode: '682',      // 682 = SAR (Saudi Riyal)
    version: '3.1',              // PayOne API version
    channel: 0,                  // 0 = Web, 1 = Mobile, 2 = Call Center
    quantity: 1,
    themeId: '1000000001'
  }
};