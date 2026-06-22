import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import * as QRCode from 'qrcode';
import { Category } from '../../../models/category.model';
import { Tool, ToolStatus } from '../../../models/tool.model';
import { CategoryService } from '../../../services/category.service';
import { ToolService } from '../../../services/tool.service';

@Component({
  selector: 'app-tool-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule
  ],
  templateUrl: './tool-form-dialog.component.html',
  styleUrl: './tool-form-dialog.component.scss'
})
export class ToolFormDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly toolService = inject(ToolService);
  private readonly categoryService = inject(CategoryService);
  private readonly dialogRef = inject(MatDialogRef<ToolFormDialogComponent>);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);
  readonly data: Tool | null = inject(MAT_DIALOG_DATA, { optional: true }) ?? null;

  readonly categories: Category[] = [];
  readonly statuses: ToolStatus[] = ['Available', 'CheckedOut', 'UnderMaintenance', 'Retired'];

  readonly form = this.fb.group({
    name: ['', Validators.required],
    make: [''],
    model: [''],
    serialNumber: [''],
    owner: [''],
    description: [''],
    barcode: [{ value: '', disabled: true }],
    location: [''],
    systainer: [''],
    imageUrl: [''],
    categoryId: [null as number | null],
    status: ['Available' as ToolStatus, Validators.required]
  });
  qrCodeDataUrl = '';

  get isEdit(): boolean {
    return !!this.data;
  }

  ngOnInit(): void {
    this.categoryService.getAll().subscribe(cats => {
      this.categories.splice(0, this.categories.length, ...cats);
    });

    if (this.data) {
      this.form.patchValue({
        name: this.data.name,
        make: this.data.make ?? '',
        model: this.data.model ?? '',
        serialNumber: this.data.serialNumber ?? '',
        owner: this.data.owner ?? '',
        description: this.data.description ?? '',
        barcode: this.data.barcode ?? '',
        location: this.data.location ?? '',
        systainer: this.data.systainer ?? '',
        imageUrl: this.data.imageUrl ?? '',
        categoryId: this.data.categoryId ?? null,
        status: this.data.status
      });
    } else {
      this.form.patchValue({ barcode: this.generateBarcode() });
    }

    this.form.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => void this.refreshQrCode());
    void this.refreshQrCode();
  }

  save(): void {
    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    const name = (value.name ?? '').trim();
    if (!name || !value.status) {
      return;
    }

    if (this.isEdit && this.data) {
      this.toolService.update(this.data.id, {
        name,
        make: value.make || undefined,
        model: value.model || undefined,
        serialNumber: value.serialNumber || undefined,
        owner: value.owner || undefined,
        description: value.description || undefined,
        location: value.location || undefined,
        systainer: value.systainer || undefined,
        imageUrl: value.imageUrl || undefined,
        categoryId: value.categoryId ?? undefined,
        status: value.status
      }).subscribe({
        next: () => {
          this.snackBar.open('Tool updated', 'OK', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: () => this.snackBar.open('Failed to update tool', 'OK', { duration: 3000 })
      });
    } else {
      this.toolService.create({
        name,
        make: value.make || undefined,
        model: value.model || undefined,
        serialNumber: value.serialNumber || undefined,
        owner: value.owner || undefined,
        description: value.description || undefined,
        barcode: value.barcode || undefined,
        location: value.location || undefined,
        systainer: value.systainer || undefined,
        imageUrl: value.imageUrl || undefined,
        categoryId: value.categoryId ?? undefined
      }).subscribe({
        next: () => {
          this.snackBar.open('Tool created', 'OK', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: () => this.snackBar.open('Failed to create tool', 'OK', { duration: 3000 })
      });
    }
  }

  cancel(): void {
    this.dialogRef.close(false);
  }

  private async refreshQrCode(): Promise<void> {
    const value = this.form.getRawValue();
    const barcode = (value.barcode ?? '').trim();
    if (!barcode) {
      this.qrCodeDataUrl = '';
      return;
    }

    const payload = JSON.stringify({
      id: barcode,
      make: (value.make ?? '').trim(),
      model: (value.model ?? '').trim(),
      owner: (value.owner ?? '').trim()
    });
    this.qrCodeDataUrl = await QRCode.toDataURL(payload, { width: 220, margin: 1 });
  }

  private generateBarcode(): string {
    if (globalThis.crypto?.randomUUID) {
      return globalThis.crypto.randomUUID().toUpperCase();
    }

    return '10000000-1000-4000-8000-100000000000'.replace(/[018]/g, c =>
      (Number(c) ^ (Math.random() * 16 >> (Number(c) / 4))).toString(16)
    ).toUpperCase();
  }
}
