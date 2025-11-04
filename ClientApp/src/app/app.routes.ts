import { Routes } from '@angular/router';
import { PaymentFormComponent } from './components/payment-form/payment-form';
import { PaymentResultComponent } from './components/payment-result/payment-result';

export const routes: Routes = [
  { path: '', component: PaymentFormComponent },
  { path: 'payment', component: PaymentFormComponent },
  { path: 'payment/success', component: PaymentResultComponent },
  { path: 'payment/failure', component: PaymentResultComponent },
  { path: '**', redirectTo: '' }
];