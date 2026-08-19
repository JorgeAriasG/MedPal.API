using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NutritionController : ControllerBase
    {
        private readonly IFoodItemRepository _foodItemRepository;
        private readonly IBodyCompositionRepository _bodyCompositionRepository;
        private readonly IAnthropometryRepository _anthropometryRepository;
        private readonly IDietPlanRepository _dietPlanRepository;
        private readonly INutritionProgressRepository _nutritionProgressRepository;
        private readonly ISupplementRepository _supplementRepository;
        private readonly INutritionService _nutritionService;
        private readonly IMapper _mapper;

        public NutritionController(
            IFoodItemRepository foodItemRepository,
            IBodyCompositionRepository bodyCompositionRepository,
            IAnthropometryRepository anthropometryRepository,
            IDietPlanRepository dietPlanRepository,
            INutritionProgressRepository nutritionProgressRepository,
            ISupplementRepository supplementRepository,
            INutritionService nutritionService,
            IMapper mapper)
        {
            _foodItemRepository = foodItemRepository;
            _bodyCompositionRepository = bodyCompositionRepository;
            _anthropometryRepository = anthropometryRepository;
            _dietPlanRepository = dietPlanRepository;
            _nutritionProgressRepository = nutritionProgressRepository;
            _supplementRepository = supplementRepository;
            _nutritionService = nutritionService;
            _mapper = mapper;
        }

        // ==================== FOOD CATALOG ====================

        [HttpGet("food")]
        public async Task<ActionResult<IEnumerable<FoodItemReadDTO>>> GetAllFoodItems()
        {
            var foodItems = await _foodItemRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<FoodItemReadDTO>>(foodItems));
        }

        [HttpGet("food/search")]
        public async Task<ActionResult<IEnumerable<FoodItemReadDTO>>> SearchFoodItems([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Ok(await _foodItemRepository.GetAllAsync());

            var foodItems = await _foodItemRepository.SearchAsync(q);
            return Ok(_mapper.Map<IEnumerable<FoodItemReadDTO>>(foodItems));
        }

        [HttpGet("food/categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetFoodCategories()
        {
            var categories = await _foodItemRepository.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("food/category/{category}")]
        public async Task<ActionResult<IEnumerable<FoodItemReadDTO>>> GetFoodItemsByCategory(string category)
        {
            var foodItems = await _foodItemRepository.GetByCategoryAsync(category);
            return Ok(_mapper.Map<IEnumerable<FoodItemReadDTO>>(foodItems));
        }

        [HttpGet("food/{id}")]
        public async Task<ActionResult<FoodItemReadDTO>> GetFoodItemById(int id)
        {
            var foodItem = await _foodItemRepository.GetByIdAsync(id);
            if (foodItem == null)
                return NotFound();

            return Ok(_mapper.Map<FoodItemReadDTO>(foodItem));
        }

        [HttpPost("food")]
        public async Task<ActionResult<FoodItemReadDTO>> CreateFoodItem([FromBody] FoodItemWriteDTO writeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var foodItem = _mapper.Map<FoodItem>(writeDto);
            foodItem.CreatedAt = DateTime.UtcNow;
            foodItem.UpdatedAt = DateTime.UtcNow;

            var created = await _foodItemRepository.AddAsync(foodItem);
            await _foodItemRepository.CompleteAsync();

            var readDto = _mapper.Map<FoodItemReadDTO>(created);
            return CreatedAtAction(nameof(GetFoodItemById), new { id = created.Id }, readDto);
        }

        [HttpPut("food/{id}")]
        public async Task<IActionResult> UpdateFoodItem(int id, [FromBody] FoodItemWriteDTO writeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var foodItem = await _foodItemRepository.GetByIdAsync(id);
            if (foodItem == null)
                return NotFound();

            _mapper.Map(writeDto, foodItem);
            foodItem.UpdatedAt = DateTime.UtcNow;

            _foodItemRepository.Update(foodItem);
            await _foodItemRepository.CompleteAsync();

            return NoContent();
        }

        [HttpDelete("food/{id}")]
        public async Task<IActionResult> DeleteFoodItem(int id)
        {
            var foodItem = await _foodItemRepository.GetByIdAsync(id);
            if (foodItem == null)
                return NotFound();

            foodItem.IsDeleted = true;
            foodItem.DeletedAt = DateTime.UtcNow;

            _foodItemRepository.Update(foodItem);
            await _foodItemRepository.CompleteAsync();

            return NoContent();
        }

        // ==================== BODY COMPOSITION ====================

        [HttpGet("body-composition/{patientDetailsId}")]
        public async Task<ActionResult<IEnumerable<BodyCompositionReadDTO>>> GetBodyCompositions(int patientDetailsId)
        {
            var records = await _bodyCompositionRepository.GetByPatientDetailsIdAsync(patientDetailsId);
            return Ok(_mapper.Map<IEnumerable<BodyCompositionReadDTO>>(records));
        }

        [HttpGet("body-composition/latest/{patientDetailsId}")]
        public async Task<ActionResult<BodyCompositionReadDTO?>> GetLatestBodyComposition(int patientDetailsId)
        {
            var record = await _bodyCompositionRepository.GetLatestAsync(patientDetailsId);
            if (record == null)
                return NoContent();

            return Ok(_mapper.Map<BodyCompositionReadDTO>(record));
        }

        [HttpPost("body-composition")]
        public async Task<ActionResult<BodyCompositionReadDTO>> CreateBodyComposition([FromBody] BodyCompositionWriteDTO writeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _mapper.Map<BodyComposition>(writeDto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            if (entity.Weight.HasValue && entity.Height.HasValue && entity.Height.Value > 0)
            {
                var heightM = entity.Height.Value / 100m;
                entity.Bmi = Math.Round(entity.Weight.Value / (heightM * heightM), 1);
            }

            var created = await _bodyCompositionRepository.AddAsync(entity);
            await _bodyCompositionRepository.CompleteAsync();

            var readDto = _mapper.Map<BodyCompositionReadDTO>(created);
            return CreatedAtAction(nameof(GetBodyCompositions), new { patientDetailsId = created.PatientDetailsId }, readDto);
        }

        [HttpPut("body-composition/{id}")]
        public async Task<IActionResult> UpdateBodyComposition(int id, [FromBody] BodyCompositionWriteDTO writeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _bodyCompositionRepository.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            _mapper.Map(writeDto, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            if (entity.Weight.HasValue && entity.Height.HasValue && entity.Height.Value > 0)
            {
                var heightM = entity.Height.Value / 100m;
                entity.Bmi = Math.Round(entity.Weight.Value / (heightM * heightM), 1);
            }

            _bodyCompositionRepository.Update(entity);
            await _bodyCompositionRepository.CompleteAsync();

            return NoContent();
        }

        [HttpDelete("body-composition/{id}")]
        public async Task<IActionResult> DeleteBodyComposition(int id)
        {
            var entity = await _bodyCompositionRepository.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;

            _bodyCompositionRepository.Update(entity);
            await _bodyCompositionRepository.CompleteAsync();

            return NoContent();
        }

        // ==================== ANTHROPOMETRY ====================

        [HttpGet("anthropometry/{patientDetailsId}")]
        public async Task<ActionResult<IEnumerable<AnthropometryReadDTO>>> GetAnthropometryRecords(int patientDetailsId)
        {
            var records = await _anthropometryRepository.GetByPatientDetailsIdAsync(patientDetailsId);
            return Ok(_mapper.Map<IEnumerable<AnthropometryReadDTO>>(records));
        }

        [HttpPost("anthropometry")]
        public async Task<ActionResult<AnthropometryReadDTO>> CreateAnthropometryRecord([FromBody] AnthropometryWriteDTO writeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _mapper.Map<AnthropometryRecord>(writeDto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            if (entity.Weight.HasValue && entity.Height.HasValue && entity.Height.Value > 0)
            {
                var heightM = entity.Height.Value / 100m;
                entity.Bmi = Math.Round(entity.Weight.Value / (heightM * heightM), 1);
                entity.WaistHeightRatio = Math.Round(entity.Waist.GetValueOrDefault() / entity.Height.Value, 2);
            }

            if (entity.Waist.HasValue && entity.Hip.HasValue && entity.Hip.Value > 0)
                entity.WaistHipRatio = Math.Round(entity.Waist.Value / entity.Hip.Value, 3);

            var created = await _anthropometryRepository.AddAsync(entity);
            await _anthropometryRepository.CompleteAsync();

            var readDto = _mapper.Map<AnthropometryReadDTO>(created);
            return CreatedAtAction(nameof(GetAnthropometryRecords), new { patientDetailsId = created.PatientDetailsId }, readDto);
        }

        [HttpPut("anthropometry/{id}")]
        public async Task<IActionResult> UpdateAnthropometryRecord(int id, [FromBody] AnthropometryWriteDTO writeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _anthropometryRepository.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            _mapper.Map(writeDto, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            if (entity.Weight.HasValue && entity.Height.HasValue && entity.Height.Value > 0)
            {
                var heightM = entity.Height.Value / 100m;
                entity.Bmi = Math.Round(entity.Weight.Value / (heightM * heightM), 1);
                entity.WaistHeightRatio = Math.Round(entity.Waist.GetValueOrDefault() / entity.Height.Value, 2);
            }

            if (entity.Waist.HasValue && entity.Hip.HasValue && entity.Hip.Value > 0)
                entity.WaistHipRatio = Math.Round(entity.Waist.Value / entity.Hip.Value, 3);

            _anthropometryRepository.Update(entity);
            await _anthropometryRepository.CompleteAsync();

            return NoContent();
        }

        [HttpDelete("anthropometry/{id}")]
        public async Task<IActionResult> DeleteAnthropometryRecord(int id)
        {
            var entity = await _anthropometryRepository.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;

            _anthropometryRepository.Update(entity);
            await _anthropometryRepository.CompleteAsync();

            return NoContent();
        }

        // ==================== DIET PLANS ====================

        [HttpGet("diet-plans/{patientDetailsId}")]
        public async Task<ActionResult<IEnumerable<DietPlanReadDTO>>> GetDietPlans(int patientDetailsId)
        {
            var dietPlans = await _dietPlanRepository.GetByPatientDetailsIdAsync(patientDetailsId);
            return Ok(_mapper.Map<IEnumerable<DietPlanReadDTO>>(dietPlans));
        }

        [HttpGet("diet-plans/detail/{id}")]
        public async Task<ActionResult<DietPlanReadDTO>> GetDietPlanById(int id)
        {
            var dietPlan = await _dietPlanRepository.GetWithMealsAsync(id);
            if (dietPlan == null)
                return NotFound();

            return Ok(_mapper.Map<DietPlanReadDTO>(dietPlan));
        }

        [HttpPost("diet-plans")]
        public async Task<ActionResult<DietPlanReadDTO>> CreateDietPlan([FromBody] DietPlanWriteDTO writeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dietPlan = _mapper.Map<DietPlan>(writeDto);
            dietPlan.CreatedAt = DateTime.UtcNow;
            dietPlan.UpdatedAt = DateTime.UtcNow;

            foreach (var meal in dietPlan.Meals)
            {
                meal.CreatedAt = DateTime.UtcNow;
                foreach (var item in meal.Items)
                {
                    item.CreatedAt = DateTime.UtcNow;
                }
            }

            var created = await _dietPlanRepository.AddAsync(dietPlan);
            await _dietPlanRepository.CompleteAsync();

            var fullPlan = await _dietPlanRepository.GetWithMealsAsync(created.Id);
            var readDto = _mapper.Map<DietPlanReadDTO>(fullPlan);
            return CreatedAtAction(nameof(GetDietPlanById), new { id = created.Id }, readDto);
        }

        [HttpPut("diet-plans/{id}")]
        public async Task<IActionResult> UpdateDietPlan(int id, [FromBody] DietPlanWriteDTO writeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _dietPlanRepository.GetWithMealsAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(writeDto, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            _dietPlanRepository.Update(existing);
            await _dietPlanRepository.CompleteAsync();

            return NoContent();
        }

        [HttpPatch("diet-plans/{id}/status")]
        public async Task<IActionResult> UpdateDietPlanStatus(int id, [FromBody] DietPlanStatusUpdateDTO statusDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dietPlan = await _dietPlanRepository.GetByIdAsync(id);
            if (dietPlan == null)
                return NotFound();

            dietPlan.Status = statusDto.Status;
            dietPlan.UpdatedAt = DateTime.UtcNow;

            _dietPlanRepository.Update(dietPlan);
            await _dietPlanRepository.CompleteAsync();

            return NoContent();
        }

        [HttpDelete("diet-plans/{id}")]
        public async Task<IActionResult> DeleteDietPlan(int id)
        {
            var dietPlan = await _dietPlanRepository.GetByIdAsync(id);
            if (dietPlan == null)
                return NotFound();

            dietPlan.IsDeleted = true;
            dietPlan.DeletedAt = DateTime.UtcNow;

            _dietPlanRepository.Update(dietPlan);
            await _dietPlanRepository.CompleteAsync();

            return NoContent();
        }

        // ==================== NUTRITION PROGRESS ====================

        [HttpGet("progress/{patientDetailsId}")]
        public async Task<ActionResult<IEnumerable<NutritionProgressReadDTO>>> GetNutritionProgress(int patientDetailsId)
        {
            var records = await _nutritionProgressRepository.GetByPatientDetailsIdAsync(patientDetailsId);
            return Ok(_mapper.Map<IEnumerable<NutritionProgressReadDTO>>(records));
        }

        [HttpPost("progress")]
        public async Task<ActionResult<NutritionProgressReadDTO>> CreateNutritionProgress([FromBody] NutritionProgressWriteDTO writeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _mapper.Map<NutritionProgress>(writeDto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            var created = await _nutritionProgressRepository.AddAsync(entity);
            await _nutritionProgressRepository.CompleteAsync();

            var readDto = _mapper.Map<NutritionProgressReadDTO>(created);
            return CreatedAtAction(nameof(GetNutritionProgress), new { patientDetailsId = created.PatientDetailsId }, readDto);
        }

        [HttpDelete("progress/{id}")]
        public async Task<IActionResult> DeleteNutritionProgress(int id)
        {
            var entity = await _nutritionProgressRepository.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;

            _nutritionProgressRepository.Update(entity);
            await _nutritionProgressRepository.CompleteAsync();

            return NoContent();
        }

        // ==================== SUPPLEMENTS ====================

        [HttpGet("supplements/{patientDetailsId}")]
        public async Task<ActionResult<IEnumerable<SupplementReadDTO>>> GetSupplements(int patientDetailsId)
        {
            var supplements = await _supplementRepository.GetByPatientDetailsIdAsync(patientDetailsId);
            return Ok(_mapper.Map<IEnumerable<SupplementReadDTO>>(supplements));
        }

        [HttpPost("supplements")]
        public async Task<ActionResult<SupplementReadDTO>> CreateSupplement([FromBody] SupplementWriteDTO writeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _mapper.Map<Supplement>(writeDto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            var created = await _supplementRepository.AddAsync(entity);
            await _supplementRepository.CompleteAsync();

            var readDto = _mapper.Map<SupplementReadDTO>(created);
            return CreatedAtAction(nameof(GetSupplements), new { patientDetailsId = created.PatientDetailsId }, readDto);
        }

        [HttpPut("supplements/{id}")]
        public async Task<IActionResult> UpdateSupplement(int id, [FromBody] SupplementWriteDTO writeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _supplementRepository.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            _mapper.Map(writeDto, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            _supplementRepository.Update(entity);
            await _supplementRepository.CompleteAsync();

            return NoContent();
        }

        [HttpDelete("supplements/{id}")]
        public async Task<IActionResult> DeleteSupplement(int id)
        {
            var entity = await _supplementRepository.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;

            _supplementRepository.Update(entity);
            await _supplementRepository.CompleteAsync();

            return NoContent();
        }

        // ==================== ASSESSMENT & INBODY ====================

        [HttpGet("assessment/{patientDetailsId}")]
        public async Task<ActionResult<NutritionAssessmentDTO>> GetAssessment(int patientDetailsId)
        {
            var assessment = await _nutritionService.CalculateAssessmentAsync(patientDetailsId);
            return Ok(assessment);
        }

        [HttpPost("inbody/sync")]
        public async Task<ActionResult<InBodySyncResultDTO>> SyncInBodyData([FromBody] InBodySyncDTO syncDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _nutritionService.SyncInBodyDataAsync(syncDto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
