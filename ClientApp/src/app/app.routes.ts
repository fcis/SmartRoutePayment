import { Routes } from '@angular/router';
import { PaymentFormComponent } from './components/payment-form/payment-form';

export const routes: Routes = [
  { path: '', component: PaymentFormComponent },
  { path: 'payment', component: PaymentFormComponent },
  { path: '**', redirectTo: '' }
];