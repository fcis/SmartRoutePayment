import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { PaymentService } from '../../services/payment.service';
import { PreparePaymentResponse } from '../../models/payment-request.model';

@Component({
  selector: 'app-payment-form',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './payment-form.html',
  styleUrls: ['./payment-form.css'],
})
export class PaymentFormComponent implements OnInit {
  @ViewChild('payoneForm', { static: false })
  payoneForm!: ElementRef<HTMLFormElement>;

  // Form data
  amount: number = 50.0; // Amount in SAR
  cardNumber: string = '';
  expiryMonth: string = '12';
  expiryYear: string = '25';
  securityCode: string = '';
  cardHolderName: string = '';
  description: string = '';
  itemId: string = '';

  // UI state
  isLoading: boolean = false;
  alertMessage: string = '';
  alertType: '' | 'success' | 'error' | 'info' = '';
  showAlert: boolean = false;

  // Payone parameters (from backend)
  paymentParams: PreparePaymentResponse | null = null;

  // Dropdown options
  months = [
    '01',
    '02',
    '03',
    '04',
    '05',
    '06',
    '07',
    '08',
    '09',
    '10',
    '11',
    '12',
  ];
  years = ['24', '25', '26', '27', '28', '29', '30'];

  constructor(private paymentService: PaymentService) {}

  ngOnInit(): void {
    // Component initialization
  }

  /**
   * Format card number with spaces
   */
  formatCardNumber(): void {
    const value = this.cardNumber.replace(/\s/g, '');
    const formatted = value.match(/.{1,4}/g)?.join(' ') || value;
    this.cardNumber = formatted;
  }

  /**
   * Handle form submission
   */
  async onSubmit(): Promise<void> {
    // Reset alert
    this.hideAlert();

    // Basic validation
    if (!this.validateForm()) {
      return;
    }

    try {
      this.isLoading = true;

      // Step 1: Call backend to prepare payment
      const prepareRequest = {
        amount: this.amount,
        paymentDescription: this.description || undefined,
        itemId: this.itemId || undefined,
        messageId: 1, // 1 = Payment
        paymentMethod: 1, // 1 = Card
      };

      this.paymentService.preparePayment(prepareRequest).subscribe({
        next: (response) => {
          if (response.success && response.data) {
            // Step 2: Store payment parameters
            this.paymentParams = response.data;

            // Step 3: Submit to Payone using hidden form
            setTimeout(() => {
              this.submitToPayone();
            }, 100);
          } else {
            this.showErrorAlert(
              response.message || 'Failed to prepare payment'
            );
            this.isLoading = false;
          }
        },
        error: (error) => {
          console.error('Payment preparation error:', error);
          this.showErrorAlert('Failed to connect to payment server');
          this.isLoading = false;
        },
      });
    } catch (error) {
      console.error('Payment error:', error);
      this.showErrorAlert('An unexpected error occurred');
      this.isLoading = false;
    }
  }

  /**
   * Submit payment data to Payone
   */
  private submitToPayone(): void {
    if (!this.paymentParams) {
      this.showErrorAlert('Payment parameters not ready');
      this.isLoading = false;
      return;
    }

    // Create hidden form and submit to Payone
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = this.paymentParams.payoneUrl;
    // Add all Payone parameters
    this.addHiddenField(form, 'MerchantId', this.paymentParams.merchantId);
    this.addHiddenField(
      form,
      'TransactionId',
      this.paymentParams.transactionId
    );
    this.addHiddenField(form, 'Amount', this.paymentParams.amount);
    this.addHiddenField(
      form,
      'CurrencyIsoCode',
      this.paymentParams.currencyIsoCode
    );
    this.addHiddenField(form, 'MessageId', this.paymentParams.messageId);
    this.addHiddenField(form, 'Quantity', this.paymentParams.quantity);
    this.addHiddenField(form, 'Channel', this.paymentParams.channel);
    this.addHiddenField(
      form,
      'PaymentMethod',
      this.paymentParams.paymentMethod
    );
    this.addHiddenField(form, 'Language', this.paymentParams.language);
    this.addHiddenField(form, 'ThemeId', this.paymentParams.themeId);
    this.addHiddenField(form, 'Version', this.paymentParams.version);
    this.addHiddenField(form, 'SecureHash', this.paymentParams.secureHash);
    // Add callback URL (where Payone redirects after payment)
    // const callbackUrl = window.location.origin + '/api/payment/callback';
    // this.addHiddenField(form, 'ResponseBackUrl', callbackUrl);
    // Add optional fields
    if (this.paymentParams.paymentDescription) {
      this.addHiddenField(
        form,
        'PaymentDescription',
        this.paymentParams.paymentDescription
      );
    }
    if (this.paymentParams.itemId) {
      this.addHiddenField(form, 'ItemId', this.paymentParams.itemId);
    }

    // Add card details (sensitive data - NOT included in SecureHash)
    this.addHiddenField(form, 'CardNumber', this.cardNumber.replace(/\s/g, ''));
    this.addHiddenField(form, 'ExpiryDateMonth', this.expiryMonth);
    this.addHiddenField(form, 'ExpiryDateYear', this.expiryYear);
    this.addHiddenField(form, 'SecurityCode', this.securityCode);
    this.addHiddenField(form, 'CardHolderName', this.cardHolderName);

    // Append form to body and submit
    document.body.appendChild(form);
    form.submit();
  

    // Note: User will be redirected to Payone payment page
    // Payment response will come back to your callback URL
  }

  /**
   * Add hidden input field to form
   */
  private addHiddenField(
    form: HTMLFormElement,
    name: string,
    value: string
  ): void {
    const input = document.createElement('input');
    input.type = 'hidden';
    input.name = name;
    input.value = value;
    form.appendChild(input);
  }

  /**
   * Validate form data
   */
  private validateForm(): boolean {
    if (this.amount <= 0) {
      this.showErrorAlert('Amount must be greater than zero');
      return false;
    }

    const cleanCardNumber = this.cardNumber.replace(/\s/g, '');
    if (
      !cleanCardNumber ||
      cleanCardNumber.length < 13 ||
      cleanCardNumber.length > 19
    ) {
      this.showErrorAlert('Please enter a valid card number');
      return false;
    }

    if (!this.expiryMonth || !this.expiryYear) {
      this.showErrorAlert('Please select expiry date');
      return false;
    }

    if (!this.securityCode || this.securityCode.length < 3) {
      this.showErrorAlert('Please enter valid CVV');
      return false;
    }

    if (!this.cardHolderName || this.cardHolderName.trim().length < 3) {
      this.showErrorAlert('Please enter cardholder name');
      return false;
    }

    return true;
  }

  /**
   * Show error alert
   */
  private showErrorAlert(message: string): void {
    this.alertMessage = message;
    this.alertType = 'error';
    this.showAlert = true;
  }

  /**
   * Show success alert
   */
  private showSuccessAlert(message: string): void {
    this.alertMessage = message;
    this.alertType = 'success';
    this.showAlert = true;
  }

  /**
   * Hide alert
   */
  private hideAlert(): void {
    this.showAlert = false;
    this.alertMessage = '';
    this.alertType = '';
  }
}
