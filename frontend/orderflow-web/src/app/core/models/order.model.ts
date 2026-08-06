export type OrderStatus = 'Pending' | 'Processing' | 'Shipped' | 'Cancelled';

export interface Order {
  id: string;
  cliente: string;
  valorTotal: number;
  status: OrderStatus;
  criadoEm: string;
}

export interface CreateOrderRequest {
  cliente: string;
  valorTotal: number;
}

export interface UpdateOrderStatusRequest {
  status: OrderStatus;
}

export const ORDER_STATUSES: readonly OrderStatus[] = [
  'Pending',
  'Processing',
  'Shipped',
  'Cancelled',
];

export const ORDER_STATUS_LABELS: Readonly<Record<OrderStatus, string>> = {
  Pending: 'Pendente',
  Processing: 'Em processamento',
  Shipped: 'Enviado',
  Cancelled: 'Cancelado',
};
