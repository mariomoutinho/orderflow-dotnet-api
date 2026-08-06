import {
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  inject,
} from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectChange, MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { finalize } from 'rxjs';
import {
  Order,
  OrderStatus,
  ORDER_STATUSES,
  ORDER_STATUS_LABELS,
} from '../../../core/models/order.model';
import { OrderService } from '../../../core/services/order.service';

@Component({
  selector: 'app-order-list',
  imports: [
    CurrencyPipe,
    DatePipe,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatFormFieldModule,
    MatIconModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSnackBarModule,
  ],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.scss',
})
export class OrderListComponent implements OnInit, OnChanges {
  private readonly orderService = inject(OrderService);
  private readonly snackBar = inject(MatSnackBar);

  @Input() refreshVersion = 0;

  readonly statuses = ORDER_STATUSES;
  readonly statusLabels = ORDER_STATUS_LABELS;

  orders: Order[] = [];
  loading = true;
  loadError: string | null = null;
  updatingOrderIds = new Set<string>();

  ngOnInit(): void {
    this.loadOrders();
  }

  ngOnChanges(changes: SimpleChanges): void {
    const refreshChange = changes['refreshVersion'];
    if (refreshChange && !refreshChange.firstChange) {
      this.loadOrders();
    }
  }

  loadOrders(): void {
    this.loading = true;
    this.loadError = null;

    this.orderService
      .getOrders()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (orders) => {
          this.orders = [...orders].sort(
            (first, second) =>
              new Date(second.criadoEm).getTime() - new Date(first.criadoEm).getTime(),
          );
        },
        error: (error: unknown) => {
          this.orders = [];
          this.loadError = this.orderService.getErrorMessage(
            error,
            'Não foi possível carregar os pedidos.',
          );
        },
      });
  }

  onStatusChange(order: Order, event: MatSelectChange): void {
    const newStatus = event.value as OrderStatus;
    if (newStatus === order.status || this.isUpdating(order.id)) {
      return;
    }

    this.setUpdating(order.id, true);
    this.orderService
      .updateOrderStatus(order.id, newStatus)
      .pipe(finalize(() => this.setUpdating(order.id, false)))
      .subscribe({
        next: (updatedOrder) => {
          this.orders = this.orders.map((currentOrder) =>
            currentOrder.id === updatedOrder.id ? updatedOrder : currentOrder,
          );
          this.snackBar.open('Status atualizado com sucesso.', 'Fechar', {
            duration: 3000,
          });
        },
        error: (error: unknown) => {
          this.orders = [...this.orders];
          const message = this.orderService.getErrorMessage(
            error,
            'Não foi possível atualizar o status.',
          );
          this.snackBar.open(message, 'Fechar', { duration: 4500 });
        },
      });
  }

  isUpdating(id: string): boolean {
    return this.updatingOrderIds.has(id);
  }

  shortId(id: string): string {
    return id.slice(0, 8).toUpperCase();
  }

  statusClass(status: OrderStatus): string {
    return `status-${status.toLowerCase()}`;
  }

  private setUpdating(id: string, updating: boolean): void {
    const nextIds = new Set(this.updatingOrderIds);
    if (updating) {
      nextIds.add(id);
    } else {
      nextIds.delete(id);
    }

    this.updatingOrderIds = nextIds;
  }
}
