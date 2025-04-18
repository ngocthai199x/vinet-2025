import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { OrderSummaryComponent } from '../../shared/components/order-summary/order-summary.component';
import { MatStepper, MatStepperModule } from '@angular/material/stepper';
import { Router, RouterLink } from '@angular/router';
import { StripeService } from '../../core/services/stripe.service';
import {
  ConfirmationToken,
  StripeAddressElement,
  StripeAddressElementChangeEvent,
  StripePaymentElement,
  StripePaymentElementChangeEvent,
} from '@stripe/stripe-js';
import { SnackbarService } from '../../core/services/snackbar.service';
import { MatButton } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import {
  MatCheckboxChange,
  MatCheckboxModule,
} from '@angular/material/checkbox';
import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { Address } from '../../shared/models/user';
import { firstValueFrom } from 'rxjs';
import { AccountService } from '../../core/services/account.service';
import { CheckoutDeliveryComponent } from './checkout-delivery/checkout-delivery.component';
import { CartService } from '../../core/services/cart.service';
import { CurrencyPipe, JsonPipe } from '@angular/common';
import { CheckoutReviewComponent } from './checkout-review/checkout-review.component';
import { OrderToCreate, ShippingAddress } from '../../shared/models/order';
import { OrderService } from '../../core/services/order.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [
    OrderSummaryComponent,
    MatStepperModule,
    RouterLink,
    MatButton,
    MatCheckboxModule,
    CheckoutDeliveryComponent,
    CheckoutReviewComponent,
    CurrencyPipe,
    MatProgressSpinnerModule,
  ],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss',
})
export class CheckoutComponent implements OnInit, OnDestroy {
  private stripeService = inject(StripeService);
  private snack = inject(SnackbarService);
  private accountService = inject(AccountService);
  private router = inject(Router);
  private orderServices = inject(OrderService);
  cartService = inject(CartService);
  addressElement?: StripeAddressElement;
  paymentElement?: StripePaymentElement;
  saveAddress = false;
  completionStatus = signal<{
    address: boolean;
    card: boolean;
    delivery: boolean;
  }>({ address: false, card: false, delivery: false });
  confimationToken?: ConfirmationToken;
  loading: boolean = false;

  async ngOnInit() {
    try {
      this.addressElement = await this.stripeService.createAddresElement();
      this.addressElement.mount('#address-element');
      this.addressElement.on('change', this.handleAddressChange);
      this.paymentElement = await this.stripeService.createPyamentElement();
      this.paymentElement.mount('#payment-element');
      this.paymentElement.on('change', this.handlePaymentChange);
    } catch (error: any) {
      this.snack.error(error.message);
    }
  }

  //using arrow function because the function will be call inside a change event of element
  handleAddressChange = (event: StripeAddressElementChangeEvent) => {
    this.completionStatus.update((state) => {
      state.address = event.complete;
      return state;
    });
  };
  handlePaymentChange = (event: StripePaymentElementChangeEvent) => {
    this.completionStatus.update((state) => {
      state.card = event.complete;
      return state;
    });
  };
  //no need arrow function here
  handleDeliveryChange(event: boolean) {
    this.completionStatus.update((state) => {
      state.delivery = event;
      return state;
    });
  }
  ngOnDestroy(): void {
    this.stripeService.disposeElement();
  }
  onSaveAddressCheckboxChange(event: MatCheckboxChange) {
    this.saveAddress = event.checked;
  }
  async getConfirmationToken() {
    try {
      if (
        Object.values(this.completionStatus()).every((status) => status == true)
      ) {
        const result = await this.stripeService.createConfirmationToken();
        if (result.error) throw new Error(result.error.message);
        this.confimationToken = result.confirmationToken;
      }
    } catch (error: any) {
      this.snack.error(error.message);
    }
  }
  async confirmPayment(stepper: MatStepper) {
    this.loading = true;
    try {
      if (this.confimationToken) {
        const result = await this.stripeService.confirmPayment(
          this.confimationToken
        );

        if (result.paymentIntent?.status === 'succeeded') {
          const order = await this.createOrderModel();
          const orderResult = await firstValueFrom(
            this.orderServices.createOrder(order)
          );
          if (orderResult) {
            this.orderServices.orderComplete = true;
            this.cartService.deleteCart();
            this.cartService.selectedDeliveryMethod.set(null);
            this.router.navigateByUrl('/checkout/success');
          } else {
            throw new Error('Order creation failed');
          }
        } else if (result.error) {
          throw new Error(result.error.message);
        } else {
          throw new Error('Something went wrong');
        }
      }
    } catch (error: any) {
      this.snack.error(error.message || 'Something went wrong');
    } finally {
      this.loading = false;
    }
  }
  private async createOrderModel(): Promise<OrderToCreate> {
    const cart = this.cartService.cart();
    const shippingAddress =
      (await this.getAddressFromTripeAddress()) as ShippingAddress;
    const card = this.confimationToken?.payment_method_preview.card;
    if (!cart?.id || !cart.deliveryMethodId || !card || !shippingAddress) {
      throw new Error('Problem creating order');
    }
    return {
      cartId: cart.id,
      paymentSummary: {
        last4: +card.last4,
        brand: card.brand,
        expMonth: card.exp_month,
        expYear: card.exp_year,
      },
      deliveryMethodId: cart.deliveryMethodId,
      shippingAddress: shippingAddress,
    };
  }
  async onStepChange(event: StepperSelectionEvent) {
    if (event.selectedIndex === 1) {
      if (this.saveAddress) {
        //get address from stripe form
        const address = (await this.getAddressFromTripeAddress()) as Address;
        address && firstValueFrom(this.accountService.updateAddress(address));
      }
    }
    if (event.selectedIndex === 2) {
      //update StripePaymentIntent
      await firstValueFrom(this.stripeService.createOrUpdatePaymentIntent());
    }
    if (event.selectedIndex === 3) {
      await this.getConfirmationToken();
      console.log(this.confimationToken);
    }
  }
  private async getAddressFromTripeAddress(): Promise<
    Address | ShippingAddress | null
  > {
    const result = await this.addressElement?.getValue();
    const address = result?.value.address;
    if (address) {
      return {
        name: result.value.name,
        line1: address.line1,
        line2: address.line2 || undefined,
        city: address.city,
        country: address.country,
        state: address.state,
        postalCode: address.postal_code,
      };
    }
    return null;
  }
}
