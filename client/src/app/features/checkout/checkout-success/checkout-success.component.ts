import {Component, inject, OnDestroy} from '@angular/core';
import {MatButton} from '@angular/material/button';
import {RouterLink} from '@angular/router';
import {SignalrService} from '../../../core/services/signalr.service';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {CurrencyPipe, DatePipe, NgIf} from '@angular/common';
import {AddressPipe} from '../../../shared/pipes/address.pipe';
import {PaymentPipe} from '../../../shared/pipes/payment.pipe';
import {OrderService} from '../../../core/services/order.service';

@Component({
  selector: 'app-checkout-success',
  imports: [
    MatButton,
    RouterLink,
    MatProgressSpinnerModule,
    DatePipe,
    AddressPipe,
    CurrencyPipe,
    PaymentPipe,
    NgIf
  ],
  templateUrl: './checkout-success.component.html',
  styleUrl: './checkout-success.component.scss'
})
export class CheckoutSuccessComponent implements OnDestroy {
  private orderService = inject(OrderService);
  signalrService = inject(SignalrService);

  ngOnDestroy(): void {
    this.orderService.orderComplete = false;
    this.signalrService.orderSignal.set(null);
  }
}