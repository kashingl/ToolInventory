import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Tool } from '../../../models/tool.model';
import { MaintenanceService } from '../../../services/maintenance.service';
import { ToolService } from '../../../services/tool.service';

@Component({
  selector: 'app-maintenance-form-dialog',
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
  templateUrl: './maintenance-form-dialog.component.html',
  styleUrl: './maintenance-form-dialog.component.scss'
})
export class MaintenanceFormDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly maintenanceService = inject(MaintenanceService);
  private readonly toolService = inject(ToolService);
  private readonly dialogRef = inject(MatDialogRef<MaintenanceFormDialogComponent>);
  private readonly snackBar = inject(MatSnackBar);

  readonly tools: Tool[] = [];

  readonly form = this.fb.group({
    toolId: [null as number | null, Validators.required],
    date: [new Date(), Validators.required],
    description: ['', Validators.required],
    performedBy: [''],
    cost: [null as number | null],
    nextMaintenanceDate: [null as Date | null]
  });

  ngOnInit(): void {
    this.toolService.getAll().subscribe(tools => {
      this.tools.splice(0, this.tools.length, ...tools);
    });
  }

  save(): void {
    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    const description = (value.description ?? '').trim();
    if (value.toolId === null || value.date === null || !description) {
      return;
    }

    this.maintenanceService.create({
      toolId: value.toolId,
      date: value.date.toISOString(),
      description,
      performedBy: value.performedBy || undefined,
      cost: value.cost ?? undefined,
      nextMaintenanceDate: value.nextMaintenanceDate?.toISOString()
    }).subscribe({
      next: () => {
        this.snackBar.open('Record created', 'OK', { duration: 3000 });
        this.dialogRef.close(true);
      },
      error: () => this.snackBar.open('Failed to create record', 'OK', { duration: 3000 })
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
