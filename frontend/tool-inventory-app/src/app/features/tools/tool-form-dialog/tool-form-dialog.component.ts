import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
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
  readonly data: Tool | null = inject(MAT_DIALOG_DATA, { optional: true }) ?? null;

  readonly categories: Category[] = [];
  readonly statuses: ToolStatus[] = ['Available', 'CheckedOut', 'UnderMaintenance', 'Retired'];

  readonly form = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    barcode: [''],
    location: [''],
    systainer: [''],
    imageUrl: [''],
    categoryId: [null as number | null],
    status: ['Available' as ToolStatus, Validators.required]
  });

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
        description: this.data.description ?? '',
        barcode: this.data.barcode ?? '',
        location: this.data.location ?? '',
        systainer: this.data.systainer ?? '',
        imageUrl: this.data.imageUrl ?? '',
        categoryId: this.data.categoryId ?? null,
        status: this.data.status
      });
    }
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
        description: value.description || undefined,
        barcode: value.barcode || undefined,
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
}
