import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import {
  CreateOrderRequest,
  Order,
  OrderStatus,
} from '../models/order.model';
import { OrderService } from './order.service';

describe('OrderService', () => {
  let service: OrderService;
  let httpTesting: HttpTestingController;

  const order: Order = {
    id: '438f41d1-0c13-42de-9c2c-9522fa33755a',
    cliente: 'Ana Silva',
    valorTotal: 149.9,
    status: 'Pending',
    criadoEm: '2026-08-06T14:30:00Z',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(OrderService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('sends GET to the orders route', () => {
    service.getOrders().subscribe((orders) => {
      expect(orders).toEqual([order]);
    });

    const request = httpTesting.expectOne('/orders');
    expect(request.request.method).toBe('GET');
    request.flush([order]);
  });

  it('sends POST with the creation payload', () => {
    const payload: CreateOrderRequest = {
      cliente: 'Ana Silva',
      valorTotal: 149.9,
    };

    service.createOrder(payload).subscribe((createdOrder) => {
      expect(createdOrder).toEqual(order);
    });

    const request = httpTesting.expectOne('/orders');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    request.flush(order);
  });

  it('sends PATCH with the status payload and order route', () => {
    const status: OrderStatus = 'Processing';

    service.updateOrderStatus(order.id, status).subscribe((updatedOrder) => {
      expect(updatedOrder.status).toBe(status);
    });

    const request = httpTesting.expectOne(`/orders/${order.id}/status`);
    expect(request.request.method).toBe('PATCH');
    expect(request.request.body).toEqual({ status });
    request.flush({ ...order, status });
  });
});
