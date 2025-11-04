import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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
}