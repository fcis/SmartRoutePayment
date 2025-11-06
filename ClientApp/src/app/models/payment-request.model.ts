export interface PreparePaymentRequest {
  amount: number;
  paymentDescription?: string;
  itemId?: string;
  messageId: number;
  paymentMethod: number;
}

export interface PreparePaymentResponse {
  transactionId: string;
  merchantId: string;
  amount: string;
  currencyIsoCode: string;
  messageId: string;
  quantity: string;
  channel: string;
  paymentMethod: string;
  language: string;
  themeId: string;
  version: string;
  secureHash: string;
  paymentDescription?: string;
  itemId?: string;
  payoneUrl: string;
  ResponseBackUrl: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}