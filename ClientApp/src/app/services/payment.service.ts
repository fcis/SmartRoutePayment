import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  PreparePaymentRequest,
  PreparePaymentResponse,
  ApiResponse
} from '../models/payment-request.model';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private apiUrl = environment.apiUrl;
  private payoneUrl = 'https://smartroute-test.payone.io/SmartRoutePaymentWeb/SRPayMsgHandler';

  constructor(private http: HttpClient) {}

  /**
   * Prepares payment by calling backend API
   * Returns all parameters needed to post to Payone
   */
  preparePayment(request: PreparePaymentRequest): Observable<ApiResponse<PreparePaymentResponse>> {
    return this.http.post<ApiResponse<PreparePaymentResponse>>(
      `${this.apiUrl}/api/payment/prepare`,
      request
    );
  }

  /**
   * Submit payment directly to Payone
   */
  submitPaymentToPayone(
    paymentParams: PreparePaymentResponse,
    cardData: {
      cardNumber: string;
      expiryMonth: string;
      expiryYear: string;
      securityCode: string;
      cardHolderName: string;
    }
  ): Observable<any> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/x-www-form-urlencoded'
    });

    const body = new URLSearchParams();
    body.set('MerchantId', paymentParams.merchantId);
    body.set('TransactionId', paymentParams.transactionId);
    body.set('Amount', paymentParams.amount);
    body.set('CurrencyIsoCode', paymentParams.currencyIsoCode);
    body.set('MessageId', paymentParams.messageId);
    body.set('Quantity', paymentParams.quantity);
    body.set('Channel', paymentParams.channel);
    body.set('PaymentMethod', paymentParams.paymentMethod);
    body.set('Language', paymentParams.language);
    body.set('ThemeId', paymentParams.themeId);
    body.set('Version', paymentParams.version);
    body.set('SecureHash', paymentParams.secureHash);
    body.set('CardNumber', cardData.cardNumber.replace(/\s/g, ''));
    body.set('ExpiryDateMonth', cardData.expiryMonth);
    body.set('ExpiryDateYear', cardData.expiryYear);
    body.set('SecurityCode', cardData.securityCode);
    body.set('CardHolderName', cardData.cardHolderName);

    if (paymentParams.paymentDescription) {
      body.set('PaymentDescription', paymentParams.paymentDescription);
    }
    if (paymentParams.itemId) {
      body.set('ItemId', paymentParams.itemId);
    }

    return this.http.post(this.payoneUrl, body.toString(), { headers });
  }
}