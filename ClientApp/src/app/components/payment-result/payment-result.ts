import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PayonePaymentResponse } from '../../models/payment-response.model';

@Component({
  selector: 'app-payment-result',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment-result.html',
  styleUrls: ['./payment-result.css']
})
export class PaymentResultComponent implements OnInit {
  isLoading: boolean = true;
  paymentResponse: PayonePaymentResponse | null = null;
  currentDate: Date = new Date();
  errorMessage: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Get all query parameters from PayOne response
    this.route.queryParams.subscribe(params => {
      console.log('Payment Response Parameters:', params);
      
      if (params && Object.keys(params).length > 0) {
        this.parsePaymentResponse(params);
      } else {
        this.errorMessage = 'No payment response data received';
      }
      
      this.isLoading = false;
    });
  }

  /**
   * Parse PayOne payment response parameters
   */
  private parsePaymentResponse(params: any): void {
    try {
      // Extract response parameters (PayOne sends them with 'Response.' prefix)
      const amount = params['Response.Amount'] || params['Amount'] || '';
      const statusCode = params['Response.StatusCode'] || params['StatusCode'] || '';
      
      // Determine if payment was successful
      const isSuccess = statusCode === '00000' || statusCode === '0';
      
      this.paymentResponse = {
        // Transaction Information
        transactionId: params['Response.TransactionID'] || params['TransactionID'] || '',
        amount: amount,
        currencyIsoCode: params['Response.CurrencyISOCode'] || params['CurrencyISOCode'] || '',
        
        // Card Information (masked)
        cardNumber: params['Response.CardNumber'] || params['CardNumber'] || '',
        cardExpiryDate: params['Response.CardExpiryDate'] || params['CardExpiryDate'] || '',
        cardHolderName: params['Response.CardHolderName'] || params['CardHolderName'] || '',
        
        // Gateway Information
        gatewayName: params['Response.GatewayName'] || params['GatewayName'] || '',
        gatewayStatusCode: params['Response.GatewayStatusCode'] || params['GatewayStatusCode'] || '',
        gatewayStatusDescription: params['Response.GatewayStatusDescription'] || params['GatewayStatusDescription'] || '',
        
        // Merchant Information
        merchantId: params['Response.MerchantID'] || params['MerchantID'] || '',
        messageId: params['Response.MessageID'] || params['MessageID'] || '',
        
        // Transaction Status
        statusCode: statusCode,
        statusDescription: this.decodeStatusDescription(
          params['Response.StatusDescription'] || params['StatusDescription'] || ''
        ),
        
        // Additional Information
        approvalCode: params['Response.ApprovalCode'] || params['ApprovalCode'] || '',
        rrn: params['Response.RRN'] || params['RRN'] || '',
        
        // Computed properties
        isSuccess: isSuccess,
        displayAmount: this.formatAmount(amount, params['Response.CurrencyISOCode'] || params['CurrencyISOCode'] || '')
      };

      console.log('Parsed Payment Response:', this.paymentResponse);
      
    } catch (error) {
      console.error('Error parsing payment response:', error);
      this.errorMessage = 'Failed to parse payment response';
      this.paymentResponse = null;
    }
  }

  /**
   * Format amount for display (convert from smallest currency unit)
   */
  private formatAmount(amount: string, currencyCode: string): string {
    if (!amount) return '0.00';
    
    // Convert from smallest unit (e.g., 50000 fills = 500.00 SAR)
    const numericAmount = parseInt(amount, 10) / 100;
    
    // Get currency symbol
    const currencySymbol = this.getCurrencySymbol(currencyCode);
    
    return `${currencySymbol} ${numericAmount.toFixed(2)}`;
  }

  /**
   * Get currency symbol based on ISO code
   */
  private getCurrencySymbol(currencyCode: string): string {
    const currencyMap: { [key: string]: string } = {
      '682': 'SAR', // Saudi Riyal
      '784': 'AED', // UAE Dirham
      '414': 'KWD', // Kuwaiti Dinar
      '048': 'BHD', // Bahraini Dinar
      // Add more as needed
    };
    
    return currencyMap[currencyCode] || 'SAR';
  }

  /**
   * Decode URL-encoded status description
   */
  private decodeStatusDescription(description: string): string {
    if (!description) return '';
    
    try {
      // Replace '+' with spaces and decode URI component
      return decodeURIComponent(description.replace(/\+/g, ' '));
    } catch (error) {
      // If decoding fails, just replace '+' with spaces
      return description.replace(/\+/g, ' ');
    }
  }

  /**
   * Check if payment was successful
   */
  get isSuccess(): boolean {
    return this.paymentResponse?.isSuccess || false;
  }

  /**
   * Get status message
   */
  get statusMessage(): string {
    if (!this.paymentResponse) return 'No response data';
    
    if (this.paymentResponse.isSuccess) {
      return 'Payment Successful';
    } else {
      return this.paymentResponse.statusDescription || 'Payment Failed';
    }
  }

  /**
   * Navigate back to payment form
   */
  backToPayment(): void {
    this.router.navigate(['/']);
  }

  /**
   * Print receipt
   */
  printReceipt(): void {
    window.print();
  }

  /**
   * Copy transaction ID to clipboard
   */
  copyTransactionId(): void {
    if (this.paymentResponse?.transactionId) {
      navigator.clipboard.writeText(this.paymentResponse.transactionId).then(() => {
        alert('Transaction ID copied to clipboard');
      });
    }
  }
}