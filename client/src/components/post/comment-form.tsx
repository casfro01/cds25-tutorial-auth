import { useNavigate } from "react-router-dom";
import { type CommentFormData } from "../../models/generated-client";
import { useForm, type SubmitHandler } from "react-hook-form";
import { blogClient } from "../../api-clients";

export default function CommentForm({ postId }: { postId: number }) {
  const navigate = useNavigate();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CommentFormData>();

  const onSubmit: SubmitHandler<CommentFormData> = async (data) => {
    await blogClient.comment(postId, data);
    navigate("");
  };

  return (
    <form method="post" onSubmit={handleSubmit(onSubmit)}>
      <div className="flex">
        <div className="flex-1">
          <textarea
            placeholder="Comment"
            className={`textarea textarea-bordered w-full ${
              errors.content && "input-error"
            }`}
            {...register("content", { required: "Content is required" })}
          ></textarea>
          <small className="text-error">{errors.content?.message}</small>
        </div>
        <div className="flex-none place-self-end pl-3">
          <button type="submit" className="btn btn-primary">
            📨 Post
          </button>
        </div>
      </div>
    </form>
  );
}
