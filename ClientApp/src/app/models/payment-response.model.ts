export interface PayonePaymentResponse {
  // Transaction Information
  transactionId: string;
  amount: string;
  currencyIsoCode: string;
  
  // Card Information (masked)
  cardNumber: string;
  cardExpiryDate: string;
  cardHolderName: string;
  
  // Gateway Information
  gatewayName: string;
  gatewayStatusCode: string;
  gatewayStatusDescription: string;
  
  // Merchant Information
  merchantId: string;
  messageId: string;
  
  // Transaction Status
  statusCode: string;
  statusDescription: string;
  
  // Additional Information
  approvalCode: string;
  rrn: string; // Retrieval Reference Number
  
  // Computed properties
  isSuccess: boolean;
  displayAmount?: string;
}

export interface PaymentResultParams {
  // These will come as query parameters from backend
  [key: string]: string;
}