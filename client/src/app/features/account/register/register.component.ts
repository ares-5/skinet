import {Component, inject} from '@angular/core';
import {MatCard} from '@angular/material/card';
import {MatButton} from '@angular/material/button';
import {FormBuilder, ReactiveFormsModule, Validators} from '@angular/forms';
import {AccountService} from '../../../core/services/account.service';
import {Router} from '@angular/router';
import {SnackbarService} from '../../../core/services/snackbar.service';
import {TextInputComponent} from '../../../shared/components/text-input/text-input.component';

@Component({
  selector: 'app-register',
  imports: [
    ReactiveFormsModule,
    MatCard,
    MatButton,
    TextInputComponent
  ],
  standalone: true,
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private accountService = inject(AccountService);
  private router = inject(Router);
  private snack = inject(SnackbarService);
  validationErrors?: string[];

  registerForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  onSubmit() {
    this.accountService.register(this.registerForm.value).subscribe({
      next: () => {
        this.snack.success('Registration successful - you can now login');
        this.router.navigateByUrl('/account/login').then(() => {});
      },
      error: errors => this.validationErrors = errors
    })
  }
}
