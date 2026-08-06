import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NEVER } from 'rxjs';
import { OrderService } from '../../../core/services/order.service';
import { OrderFormComponent } from './order-form.component';

describe('OrderFormComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrderFormComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
  });

  it('does not create an order when the form is invalid', () => {
    const fixture = TestBed.createComponent(OrderFormComponent);
    const component = fixture.componentInstance;
    const service = TestBed.inject(OrderService);
    const createOrder = vi.spyOn(service, 'createOrder').mockReturnValue(NEVER);

    component.onSubmit();

    expect(component.form.invalid).toBe(true);
    expect(createOrder).not.toHaveBeenCalled();
  });
});
