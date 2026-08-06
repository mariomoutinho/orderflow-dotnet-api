import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateOrderRequest,
  Order,
  OrderStatus,
  UpdateOrderStatusRequest,
} from '../models/order.model';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly ordersUrl = '/orders';

  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(this.ordersUrl);
  }

  createOrder(request: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(this.ordersUrl, request);
  }

  updateOrderStatus(id: string, status: OrderStatus): Observable<Order> {
    const request: UpdateOrderStatusRequest = { status };
    return this.http.patch<Order>(
      `${this.ordersUrl}/${encodeURIComponent(id)}/status`,
      request,
    );
  }

  getErrorMessage(error: unknown, fallback: string): string {
    if (!(error instanceof HttpErrorResponse)) {
      return fallback;
    }

    if (error.status === 0) {
      return 'A API parece estar indisponível.';
    }

    if (error.status === 400) {
      return 'Confira os dados informados.';
    }

    if (error.status === 404) {
      return 'Pedido não encontrado.';
    }

    return fallback;
  }
}
