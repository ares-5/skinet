import {inject, Injectable} from '@angular/core';
import {Order, OrderToCreate} from '../../shared/models/order';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);
  orderComplete = false;

  createOrder(orderToCreate: OrderToCreate) {
    return this.http.post<Order>(`${this.baseUrl}/orders`, orderToCreate);
  }

  getOrdersForUser() {
    return this.http.get<Order[]>(`${this.baseUrl}/orders`);
  }

  getOrderDetailed(id: number) {
    return this.http.get<Order>(`${this.baseUrl}/orders/${id}`);
  }
}
