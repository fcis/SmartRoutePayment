import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-payment-result',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment-result.html',
  styleUrls: ['./payment-result.css']
})
export class PaymentResultComponent implements OnInit {
  isSuccess: boolean = false;
  transactionId: string = '';
  errorMessage: string = '';
  isLoading: boolean = true;
 currentDate: Date = new Date();
  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Determine if this is success or failure page
    this.isSuccess = this.router.url.includes('/success');

    // Get query parameters
    this.route.queryParams.subscribe(params => {
      this.transactionId = params['transactionId'] || '';
      this.errorMessage = params['error'] || '';
      this.isLoading = false;
    });
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
}