import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Tool } from '../../../models/tool.model';
import { CheckoutService } from '../../../services/checkout.service';
import { ToolService } from '../../../services/tool.service';

@Component({
  selector: 'app-checkout-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSnackBarModule
  ],
  templateUrl: './checkout-dialog.component.html',
  styleUrl: './checkout-dialog.component.scss'
})
export class CheckoutDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly checkoutService = inject(CheckoutService);
  private readonly toolService = inject(ToolService);
  private readonly dialogRef = inject(MatDialogRef<CheckoutDialogComponent>);
  private readonly snackBar = inject(MatSnackBar);
  readonly data: Tool | null = inject(MAT_DIALOG_DATA, { optional: true }) ?? null;

  readonly availableTools: Tool[] = [];

  readonly form = this.fb.group({
    toolId: [null as number | null, Validators.required],
    userId: ['', [Validators.required, Validators.maxLength(450)]],
    expectedReturnDate: [null as Date | null],
    notes: ['']
  });

  ngOnInit(): void {
    this.toolService.getAll().subscribe(tools => {
      const availableTools = tools.filter(tool => tool.status === 'Available');

      if (this.data && !availableTools.some(tool => tool.id === this.data?.id)) {
        availableTools.unshift(this.data);
      }

      this.availableTools.splice(0, this.availableTools.length, ...availableTools);
    });

    if (this.data) {
      this.form.patchValue({ toolId: this.data.id });
    }
  }

  save(): void {
    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    const userId = (value.userId ?? '').trim();
    if (value.toolId === null || !userId) {
      return;
    }

    this.checkoutService.create({
      toolId: value.toolId,
      userId,
      expectedReturnDate: value.expectedReturnDate?.toISOString(),
      notes: value.notes || undefined
    }).subscribe({
      next: () => {
        this.snackBar.open('Tool checked out', 'OK', { duration: 3000 });
        this.dialogRef.close(true);
      },
      error: () => this.snackBar.open('Failed to checkout tool', 'OK', { duration: 3000 })
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
