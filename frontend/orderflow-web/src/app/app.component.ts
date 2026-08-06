import { Component, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { OrderFormComponent } from './features/orders/order-form/order-form.component';
import { OrderListComponent } from './features/orders/order-list/order-list.component';

@Component({
  selector: 'app-root',
  imports: [
    MatIconModule,
    MatToolbarModule,
    OrderFormComponent,
    OrderListComponent,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {
  readonly refreshVersion = signal(0);

  refreshOrders(): void {
    this.refreshVersion.update((version) => version + 1);
  }
}
