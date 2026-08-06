import { Component, EventEmitter, inject, Output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { finalize } from 'rxjs';
import { Order } from '../../../core/models/order.model';
import { OrderService } from '../../../core/services/order.service';

@Component({
  selector: 'app-order-form',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './order-form.component.html',
  styleUrl: './order-form.component.scss',
})
export class OrderFormComponent {
  private readonly orderService = inject(OrderService);
  private readonly snackBar = inject(MatSnackBar);

  @Output() readonly orderCreated = new EventEmitter<Order>();

  readonly form = new FormGroup({
    cliente: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/\S/)],
    }),
    valorTotal: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(0.01),
    ]),
  });

  submitting = false;

  onSubmit(): void {
    this.form.markAllAsTouched();

    if (this.form.invalid || this.submitting) {
      return;
    }

    const cliente = this.form.controls.cliente.value.trim();
    const valorTotal = this.form.controls.valorTotal.value;

    if (!cliente || valorTotal === null) {
      return;
    }

    this.submitting = true;
    this.orderService
      .createOrder({ cliente, valorTotal })
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe({
        next: (order) => {
          this.form.reset({ cliente: '', valorTotal: null });
          this.orderCreated.emit(order);
          this.snackBar.open('Pedido criado com sucesso.', 'Fechar', {
            duration: 3000,
          });
        },
        error: (error: unknown) => {
          const message = this.orderService.getErrorMessage(
            error,
            'Não foi possível criar o pedido.',
          );
          this.snackBar.open(message, 'Fechar', { duration: 4500 });
        },
      });
  }
}
